// Captures the screenshots in docs/images by starting the real application and driving it in a
// browser. Nothing here is staged: every picture in the README is of this repository's own code
// serving its own seed data.
//
//   cd tools && npm install && npx playwright install chromium
//   node capture-screenshots.js [--port 5188]
//
// Starting the app end to end is also a test in its own right. It exercises startup, migration,
// seeding, routing, static files and layout -- paths no unit test touches, and where a first-run
// crash would be the first thing a reader hit.

const { spawn } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const repoRoot = path.resolve(__dirname, '..');
const outputDirectory = path.join(repoRoot, 'docs', 'images');
const port = readPort(process.argv.slice(2));
const baseUrl = `http://localhost:${port}`;

async function main() {
    fs.mkdirSync(outputDirectory, { recursive: true });

    const app = startApplication();
    try {
        await waitForCatalogue();
        await capture();
    } finally {
        stop(app);
    }
}

function readPort(args) {
    const index = args.indexOf('--port');
    return index >= 0 && args[index + 1] ? Number(args[index + 1]) : 5188;
}

function startApplication() {
    console.log(`Starting the application on ${baseUrl} ...`);

    return spawn(
        'dotnet',
        ['run', '--project', path.join('src', 'QwestRooms.UI'), '-c', 'Release', '--urls', baseUrl],
        {
            cwd: repoRoot,
            stdio: 'ignore',
            shell: process.platform === 'win32',
            detached: process.platform !== 'win32'
        });
}

function stop(app) {
    if (!app || app.exitCode !== null) {
        return;
    }

    // A console app started through a shell outlives its parent and keeps a handle on bin/, so
    // the whole tree has to go, not just the process that was spawned.
    if (process.platform === 'win32') {
        spawn('taskkill', ['/T', '/F', '/PID', String(app.pid)], { stdio: 'ignore' });
    } else {
        process.kill(-app.pid, 'SIGTERM');
    }
}

async function waitForCatalogue() {
    const deadline = Date.now() + 180_000;

    while (Date.now() < deadline) {
        try {
            const response = await fetch(`${baseUrl}/healthz`);
            if (response.ok) {
                const health = await response.json();
                if (health.rooms > 0) {
                    console.log(`Catalogue is up with ${health.rooms} rooms.`);
                    return;
                }
            }
        } catch {
            // Not listening yet.
        }

        await new Promise(resolve => setTimeout(resolve, 500));
    }

    throw new Error(`${baseUrl}/healthz never reported a seeded catalogue.`);
}

async function capture() {
    const browser = await chromium.launch();
    const context = await browser.newContext({
        viewport: { width: 1440, height: 900 },
        deviceScaleFactor: 2,
        reducedMotion: 'no-preference'
    });
    const page = await context.newPage();

    const failures = [];
    page.on('pageerror', error => failures.push(`page error: ${error.message}`));
    page.on('requestfailed', request => failures.push(`failed request: ${request.url()}`));

    await page.goto(baseUrl, { waitUntil: 'networkidle' });
    await page.waitForSelector('.flip-card');

    await shoot(page, 'catalogue.png');

    // The filter, driven the way a visitor drives it: country, then city, then Apply.
    await page.click('[data-bs-target="#filterPanel"]');
    await page.waitForSelector('#filterPanel.show');
    await page.click('#countryButton');
    await page.click('.js-country:has-text("Ukraine")');
    await page.waitForSelector('#cityButton:not([disabled])');
    await page.click('#cityButton');
    await page.click('.js-city:has-text("Kyiv")');
    await page.waitForSelector('#addressButton:not([disabled])');
    await page.click('#applyFilter');
    await page.waitForFunction(
        () => document.getElementById('resultCount').textContent.trim() !== '450 rooms');

    const filtered = await page.textContent('#resultCount');
    console.log(`Filtered to ${filtered.trim()}.`);
    await shoot(page, 'filter.png');

    // One card mid-flip, to show what the back holds. The transition is 0.7s.
    await page.click('#clearFilter');
    await page.waitForFunction(
        () => document.getElementById('resultCount').textContent.trim() === '450 rooms');
    const cards = page.locator('.flip-card');
    await cards.nth(1).hover();
    await page.waitForTimeout(1200);
    await cards.nth(1).screenshot({ path: path.join(outputDirectory, 'card.png') });
    console.log('Wrote card.png');

    // The 2019 grid was floated fixed-width cards, so a phone got a horizontal scrollbar. Checked
    // rather than claimed, and photographed while we are here.
    const phone = await context.newPage();
    await phone.setViewportSize({ width: 390, height: 844 });
    await phone.goto(baseUrl, { waitUntil: 'networkidle' });
    await phone.waitForSelector('.flip-card');

    const overflow = await phone.evaluate(
        () => document.documentElement.scrollWidth - document.documentElement.clientWidth);
    console.log(`Horizontal overflow at 390px: ${overflow}px`);
    if (overflow > 0) {
        failures.push(`the page overflows sideways by ${overflow}px on a 390px viewport`);
    }

    await phone.screenshot({
        path: path.join(outputDirectory, 'phone.png'),
        clip: { x: 0, y: 0, width: 390, height: 844 }
    });
    console.log('Wrote phone.png');

    await browser.close();

    if (failures.length > 0) {
        throw new Error(`The page reported problems while being photographed:\n${failures.join('\n')}`);
    }
}

async function shoot(page, name) {
    // Clipped to the viewport rather than full-page: 27 cards make a very tall picture that
    // reads as nothing at README width.
    await page.screenshot({
        path: path.join(outputDirectory, name),
        clip: { x: 0, y: 0, width: 1440, height: 900 }
    });
    console.log(`Wrote ${name}`);
}

main().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
