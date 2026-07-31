<#
.SYNOPSIS
    Regenerates the demo dataset: the MockData SQL scripts and the room poster artwork.

.DESCRIPTION
    The original seed data was randomly generated without constraints, which produced values that
    were individually plausible but collectively nonsense: ratings and difficulty on a 1-100
    scale, escape rooms lasting twenty hours, and cities scattered across unrelated countries.

    This script generates a coherent dataset instead. Cities and streets belong to their own
    country, rooms are built from themed templates, and every numeric field uses the range the UI
    actually implies. The random seed is fixed, so re-running it reproduces the same data.

    Run it, then reseed:
        .\tools\generate-seed.ps1
        .\tools\dev.ps1 reseed
        .\tools\dev.ps1 run

    The SQL is deliberately dialect-neutral -- unquoted identifiers, no batch separators, no
    identity-insert -- so the same scripts load into SQLite today and would load into SQL Server
    unchanged.
#>
[CmdletBinding()]
param(
    [int]$RoomsPerCountry = 30,
    [int]$Seed = 20260727
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$DataDir  = Join-Path $RepoRoot 'src\QwestRooms.DAL\MockData'
$ArtDir   = Join-Path $RepoRoot 'src\QwestRooms.UI\wwwroot\img\rooms'

$random = [System.Random]::new($Seed)
function Pick($items) { return $items[$random.Next(0, $items.Count)] }
function PickInt($min, $max) { return $random.Next($min, $max + 1) }
function SqlEscape($text) { return $text.Replace("'", "''") }

# --------------------------------------------------------------------------------------------
# Geography. Each country carries its own cities and streets, so no address can pair a city with
# a country it does not belong to.
# --------------------------------------------------------------------------------------------
$geography = @(
    @{ Country = 'Ukraine';        Cities = @('Kyiv','Lviv','Odesa','Kharkiv','Dnipro');            Streets = @('Khreshchatyk','Shevchenka','Franka','Hrushevskoho','Sadova') }
    @{ Country = 'Poland';         Cities = @('Warsaw','Krakow','Gdansk','Wroclaw','Poznan');       Streets = @('Marszalkowska','Dluga','Piotrkowska','Florianska','Krakowska') }
    @{ Country = 'Germany';        Cities = @('Berlin','Munich','Hamburg','Cologne','Frankfurt');   Streets = @('Hauptstrasse','Bahnhofstrasse','Schillerstrasse','Goethestrasse','Lindenallee') }
    @{ Country = 'France';         Cities = @('Paris','Lyon','Marseille','Toulouse','Nice');        Streets = @('Rue de la Paix','Avenue Victor Hugo','Rue Saint-Denis','Boulevard Voltaire','Rue des Lilas') }
    @{ Country = 'Spain';          Cities = @('Madrid','Barcelona','Valencia','Seville','Bilbao');  Streets = @('Calle Mayor','Gran Via','Calle de Alcala','Paseo del Prado','Rambla Nova') }
    @{ Country = 'Italy';          Cities = @('Rome','Milan','Naples','Turin','Florence');          Streets = @('Via Roma','Via Garibaldi','Corso Buenos Aires','Via Dante','Via Manzoni') }
    @{ Country = 'Netherlands';    Cities = @('Amsterdam','Rotterdam','Utrecht','The Hague','Eindhoven'); Streets = @('Kalverstraat','Damrak','Prinsengracht','Coolsingel','Oudegracht') }
    @{ Country = 'Czech Republic'; Cities = @('Prague','Brno','Ostrava','Plzen','Olomouc');         Streets = @('Vaclavske namesti','Parizska','Narodni','Masarykova','Ceska') }
    @{ Country = 'United Kingdom'; Cities = @('London','Manchester','Birmingham','Edinburgh','Bristol'); Streets = @('High Street','Baker Street','Kings Road','Victoria Street','Queens Road') }
    @{ Country = 'Ireland';        Cities = @('Dublin','Cork','Galway','Limerick');                 Streets = @('Grafton Street','O Connell Street','Patrick Street','Shop Street') }
    @{ Country = 'Sweden';         Cities = @('Stockholm','Gothenburg','Malmo','Uppsala');          Streets = @('Drottninggatan','Kungsgatan','Storgatan','Vasagatan') }
    @{ Country = 'Norway';         Cities = @('Oslo','Bergen','Trondheim','Stavanger');             Streets = @('Karl Johans gate','Storgata','Bryggen','Kirkegata') }
    @{ Country = 'Portugal';       Cities = @('Lisbon','Porto','Braga','Coimbra');                  Streets = @('Rua Augusta','Avenida da Liberdade','Rua de Santa Catarina','Rua do Carmo') }
    @{ Country = 'Austria';        Cities = @('Vienna','Graz','Salzburg','Linz');                   Streets = @('Karntner Strasse','Mariahilfer Strasse','Getreidegasse','Landstrasse') }
    @{ Country = 'Hungary';        Cities = @('Budapest','Debrecen','Szeged','Pecs');               Streets = @('Andrassy ut','Vaci utca','Rakoczi ut','Kossuth ter') }
)

# --------------------------------------------------------------------------------------------
# Room themes. Each drives a name, a description, an artwork file and a plausible fear level.
# --------------------------------------------------------------------------------------------
$themes = @(
    @{ Key='space';      Title='Space';        Fear=2; Names=@('Orbital Silence','The Cosmonaut''s Last Signal','Docking Bay Seven','Beacon in the Void','Return Trajectory')
       Blurb='A research station has stopped answering. You have one orbit to restore the beacon before the window closes and the station drifts out of contact for good.' }
    @{ Key='prison';     Title='Prison Break'; Fear=3; Names=@('Cell Block Nine','The Long Yard','Lights Out','Warden''s Ledger','Two Hours Till Roll Call')
       Blurb='Roll call is in an hour. The guard rotation has one gap, the ledger in the warden''s office has the tunnel map, and only one of you can carry the keys.' }
    @{ Key='tomb';       Title='Lost Tomb';    Fear=3; Names=@('The Sealed Cartouche','Chamber of the Scribe','Sand Over the Door','The Fourth Sarcophagus','Nile Below')
       Blurb='The dig team sealed this chamber for a reason. Their notes stop mid-sentence, the air is thinning, and the door mechanism only answers to the old alphabet.' }
    @{ Key='asylum';     Title='Asylum';       Fear=5; Names=@('Ward Twelve','The Night Register','Patient Unknown','Sedative Hour','The Quiet Corridor')
       Blurb='The ward closed in 1961 and the night register was never finished. Somebody kept writing in it anyway, and the last entry has today''s date.' }
    @{ Key='heist';      Title='Bank Heist';   Fear=1; Names=@('The Vault at Midnight','Six Minutes of Silence','Deposit Box 114','Clean Getaway','The Manager''s Routine')
       Blurb='The alarm loop resets every six minutes. That is how long you have inside the vault, and box 114 is at the back behind two more locks than the plans promised.' }
    @{ Key='detective';  Title='Detective';    Fear=2; Names=@('The Rain-Soaked Alibi','Case File 47','Last Call at the Blue Room','What the Landlady Saw','Three Witnesses, One Lie')
       Blurb='Three witnesses, three statements, and only one of them can be true. The case file is on the desk, the clock is on the wall, and the precinct wants an answer tonight.' }
    @{ Key='laboratory'; Title='Laboratory';   Fear=4; Names=@('Containment Level Four','The Culture Sample','Reagent Missing','Cold Storage','Trial Twenty-Two')
       Blurb='Containment failed at 04:12 and the lockdown sealed the wing with the sample still inside. The trial notes explain how to neutralise it, if you can read the handwriting.' }
    @{ Key='pirate';     Title='Pirate Ship';  Fear=2; Names=@('Below the Waterline','The Quartermaster''s Share','Charts and Cutlasses','Mutiny at Dawn','The Salt-Stained Log')
       Blurb='The captain is ashore, the quartermaster has hidden the share, and the tide turns at dawn. The ship''s log has the bearings, but half its pages are salt-ruined.' }
    @{ Key='submarine';  Title='Submarine';    Fear=4; Names=@('Depth Charge','Compartment Four Is Flooding','Silent Running','The Sonar Ghost','Surface in Sixty')
       Blurb='Compartment four is flooding, the sonar shows something that should not be there, and the manual for the ballast override is in the compartment that is flooding.' }
    @{ Key='wizard';     Title='Wizard''s Study'; Fear=1; Names=@('The Unfinished Spell','Ink, Salt and Silver','The Apprentice''s Mistake','Nine Candles','The Locked Grimoire')
       Blurb='The apprentice mixed the reagents in the wrong order and the spell is still half-cast. Nine candles are lit; when the last goes out, whatever is half-here becomes whole.' }
    @{ Key='bunker';     Title='Cold War Bunker'; Fear=3; Names=@('Launch Code Rescinded','The Duty Officer''s Chair','Forty Metres Down','Signal from Moscow','The Second Key')
       Blurb='Two keys, two officers, one order that was rescinded eleven minutes ago. The confirmation never arrived and the duty officer has left the room.' }
    @{ Key='time';       Title='Time Machine'; Fear=2; Names=@('Paradox Hour','The Same Room, Yesterday','Chronometer Drift','You Have Been Here Before','Reset the Loop')
       Blurb='You have solved this room before. There is a note in your own handwriting saying so, and the chronometer is drifting further every time the loop restarts.' }
    @{ Key='museum';     Title='Museum';       Fear=1; Names=@('After Closing','The Forgery in Gallery Two','Provenance Unknown','The Curator''s Confession','Glass Case Nineteen')
       Blurb='One painting in gallery two is a forgery and the curator knows which. The provenance files are locked, the guard does his round every twelve minutes, and closing was an hour ago.' }
    @{ Key='outbreak';   Title='Outbreak';     Fear=5; Names=@('Quarantine Wing','Patient Zero''s Route','The Last Clean Room','Forty-Eight Hours','Do Not Open The Door')
       Blurb='The quarantine held for six days. Patient zero''s route is mapped on the wall in marker, and the last clean room is three corridors away through everything that went wrong.' }
    @{ Key='fairytale';  Title='Fairytale';    Fear=1; Names=@('The Thorn Gate','Twelve Dancing Doors','Breadcrumbs','The Miller''s Bargain','What the Wolf Left')
       Blurb='The bargain was struck fairly, which is the problem. The thorn gate opens for the price agreed, and the miller never read past the first page of the contract.' }
    @{ Key='steampunk';  Title='Steampunk';    Fear=2; Names=@('The Brass Regulator','Pressure Rising','Cogwright''s Workshop','Steam Before Sunrise','The Automaton''s Ledger')
       Blurb='The regulator is over pressure and the workshop''s automaton has locked the release behind its own ledger, which it writes in a cipher of its own devising.' }
)

# One description per room concept rather than per theme. With a single blurb per theme, any page
# showing two rooms of the same theme printed the same paragraph twice, which read as generated.
$roomBlurbs = @{
    'Orbital Silence'              = 'The station stopped answering three orbits ago, and its final transmission was a single repeated digit.'
    "The Cosmonaut's Last Signal"  = 'One bunk is still warm, one suit is missing, and the log ends midway through a sentence about the airlock.'
    'Docking Bay Seven'            = 'The approach vector is wrong, the bay doors answer to a code nobody wrote down, and fuel is measured in minutes.'
    'Beacon in the Void'           = 'Restore the beacon before the orbit carries you out of range, because the next window is eleven months away.'
    'Return Trajectory'            = 'The burn must happen to the second, and the navigation computer disagrees with the charts. One of them is lying.'

    'Cell Block Nine'              = 'Roll call is in an hour, the guard rotation has exactly one gap, and the keys are two doors further than you were promised.'
    'The Long Yard'                = 'The tunnel surfaces under the yard floodlights, so timing the walk matters rather more than digging it did.'
    'Lights Out'                   = 'You have from lights out until first light, and the distance to the laundry is counted in footsteps, not metres.'
    "Warden's Ledger"              = 'Everything you need is in the warden''s ledger, which is precisely why he keeps it locked in the room you are standing in.'
    'Two Hours Till Roll Call'     = 'Two hours, three locks, and a cellmate who has done this before and will not say how it ended.'

    'The Sealed Cartouche'         = 'The cartouche was cut to be read once and then buried. The dig team never got that far.'
    'Chamber of the Scribe'        = 'The scribe recorded everything, including how to leave, in an alphabet that died with him.'
    'Sand Over the Door'           = 'Sand is coming in faster than you can clear it, and the door mechanism only turns while it is dry.'
    'The Fourth Sarcophagus'       = 'Three sarcophagi appear in the records. The fourth does not, and it is the one standing open.'
    'Nile Below'                   = 'The lower chamber floods on a schedule the builders set three thousand years ago and never wrote down.'

    'Ward Twelve'                  = 'Ward twelve closed in 1961, and somebody has kept the night register current ever since.'
    'The Night Register'           = 'Every entry is a name and a time. The last one is tonight, and the handwriting is unfamiliar.'
    'Patient Unknown'              = 'One file carries no name and no photograph, and a treatment schedule that ran for nineteen years.'
    'Sedative Hour'                = 'The dispensary clock still keeps the old rounds, and the corridor lights dim whenever it strikes.'
    'The Quiet Corridor'           = 'Staff called it the quiet corridor because nothing was ever heard from it, which is not the same as nothing happening.'

    'The Vault at Midnight'        = 'The alarm loop resets every six minutes. That is how long you have inside, and how long you have to get out.'
    'Six Minutes of Silence'       = 'Six minutes of looped camera feed, bought at considerable expense, and a safe that takes eight to open honestly.'
    'Deposit Box 114'              = 'Box 114 sits at the back, behind two more locks than the floor plan admitted to.'
    'Clean Getaway'                = 'Getting in was the part you planned. The car outside belongs to somebody who has just noticed it is missing.'
    "The Manager's Routine"        = 'The manager is predictable to the minute, which is useful right up until the evening he is not.'

    'The Rain-Soaked Alibi'        = 'The alibi holds unless it rained that night, and the weather report is the one page missing from the file.'
    'Case File 47'                 = 'Forty-six cases closed cleanly. This one has been reopened four times by four different detectives.'
    'Last Call at the Blue Room'   = 'Everyone at the bar agrees on when it happened, and nobody agrees on who walked out first.'
    'What the Landlady Saw'        = 'She saw all of it from the second-floor window and will tell you every part except the one that matters.'
    'Three Witnesses, One Lie'     = 'Three statements, mutually exclusive, and a precinct that wants a name before morning.'

    'Containment Level Four'       = 'Containment failed at 04:12 and the lockdown sealed the wing with the sample still inside it.'
    'The Culture Sample'           = 'It was meant to stay dormant below four degrees. The cold storage door has stood open since Friday.'
    'Reagent Missing'              = 'The neutralising agent is logged as used and signed for by a technician who does not work here.'
    'Cold Storage'                 = 'The freezer inventory and the freezer contents have disagreed for six weeks, and only one of them is written down.'
    'Trial Twenty-Two'             = 'Twenty-one trials with clean write-ups, and a twenty-second that stops after the opening paragraph.'

    'Below the Waterline'          = 'Whatever the quartermaster hid, he hid below the waterline, and the hold is already taking water.'
    "The Quartermaster's Share"    = 'The share was counted twice and came out different both times. That is how the trouble started.'
    'Charts and Cutlasses'         = 'The charts are accurate and the bearings are not, because somebody wanted this island to stay lost.'
    'Mutiny at Dawn'               = 'Half the crew has agreed to it and the other half has not been asked. Dawn is one turn of the glass away.'
    'The Salt-Stained Log'         = 'The log holds the bearings, but the pages you need have been in seawater since the last storm.'

    'Depth Charge'                 = 'Somewhere above, a ship is dropping charges to a pattern, and the pattern is tightening.'
    'Compartment Four Is Flooding' = 'The manual for the ballast override is stowed in compartment four, which is the compartment that is flooding.'
    'Silent Running'               = 'No engines, no lights, no talking, and a leak making more noise than all of you together.'
    'The Sonar Ghost'              = 'The contact has held station off the port bow for six hours and matches nothing in the recognition book.'
    'Surface in Sixty'             = 'Air is calculated for sixty minutes, the hatch is jammed, and the calculation assumed four people rather than six.'

    'The Unfinished Spell'         = 'The apprentice added the reagents in the wrong order, and the spell has been halfway cast since Tuesday.'
    'Ink, Salt and Silver'         = 'Three things hold the circle closed, and one of them has been quietly running out all evening.'
    "The Apprentice's Mistake"     = 'It was a small mistake, carefully made, and it is now standing on the other side of the door.'
    'Nine Candles'                 = 'Nine were lit at dusk. When the last goes out, whatever is half here finishes arriving.'
    'The Locked Grimoire'          = 'The book has locked itself, which the master insists is a safety feature rather than a problem.'

    'Launch Code Rescinded'        = 'The order was rescinded eleven minutes ago and the confirmation has still not arrived.'
    "The Duty Officer's Chair"     = 'Two keys, two officers, and one chair that has stood empty since the alert began.'
    'Forty Metres Down'            = 'Forty metres of concrete between you and the surface, and a lift that wants an authorisation you do not have.'
    'Signal from Moscow'           = 'The signal arrived clean, decoded to nonsense, and the codebook was destroyed in accordance with procedure.'
    'The Second Key'               = 'The system needs both keys turned together, and the second officer left the bunker an hour ago.'

    'Paradox Hour'                 = 'There is a note in your own handwriting explaining what not to do, and you have already done it.'
    'The Same Room, Yesterday'     = 'The room resets at the same moment each cycle, and the chronometer drifts a little further every time.'
    'Chronometer Drift'            = 'The drift was three seconds a loop. It is now four minutes, and the window is closing.'
    'You Have Been Here Before'    = 'The evidence that you have been here before is thorough, dated, and unmistakably yours.'
    'Reset the Loop'               = 'Breaking the loop means doing everything in reverse, once, without a single mistake.'

    'After Closing'                = 'The building empties at six and the alarms arm at seven. You have the hour in between.'
    'The Forgery in Gallery Two'   = 'One canvas in gallery two is not what its label claims, and the curator has known for years.'
    'Provenance Unknown'           = 'The acquisition file contains a gap of four decades that nobody has ever been asked to explain.'
    "The Curator's Confession"     = 'He wrote all of it down and locked it inside the case holding the object he was confessing about.'
    'Glass Case Nineteen'          = 'Case nineteen is alarmed separately, on a circuit that appears on no plan of the building.'

    'Quarantine Wing'              = 'The quarantine held for six days on a design rated for two.'
    "Patient Zero's Route"         = 'Somebody mapped the route in marker along the corridor wall and did not finish the last stretch.'
    'The Last Clean Room'          = 'Three corridors lie between here and the clean room, and the doors open only one at a time.'
    'Forty-Eight Hours'            = 'The incubation period is forty-eight hours, and nobody is certain when the clock started.'
    'Do Not Open The Door'         = 'The instruction is written on both sides of the door, in two different hands.'

    'The Thorn Gate'               = 'The gate opens for the price agreed, and the price was agreed before anybody read it aloud.'
    'Twelve Dancing Doors'         = 'Twelve doors, each worn through at the threshold, and no record of anyone ever using them.'
    'Breadcrumbs'                  = 'The trail was laid carefully, and something has been eating it since the light went.'
    "The Miller's Bargain"         = 'The bargain was struck fairly and in good faith, on terms the miller never read past the first page.'
    'What the Wolf Left'           = 'The cottage is tidy and the table is set for two, but only one place has been used.'

    'The Brass Regulator'          = 'The regulator is over pressure and the release is locked behind the workshop''s own bookkeeping.'
    'Pressure Rising'              = 'The gauge passed the red line an hour ago, and the workshop has been growing warmer ever since.'
    "Cogwright's Workshop"         = 'Everything here was built by one man to be understood by one man, and he retired without leaving notes.'
    'Steam Before Sunrise'         = 'The boiler must be vented before sunrise, and the vent key is part of the mechanism it opens.'
    "The Automaton's Ledger"       = 'The automaton keeps its accounts in a cipher of its own devising and will not be hurried.'
}

$companySuffixes = @('Escape Rooms','Quest Rooms','Escape Company','Adventure Rooms','Live Games','Puzzle House')
$companyPrefixes = @('Nebula','Cipher','Keyhole','Blackbox','Lantern','Redstone','North Gate','Hourglass','Ironwood','Paper Lantern',
                     'Copper Fox','Mind Vault','Sealed Door','Silver Compass','Thirteenth Hour','Blue Room','Foxglove','Wayfarer')

# --------------------------------------------------------------------------------------------
# Poster artwork: one SVG per theme, generated so the repo owns its own images and nothing is
# hotlinked. The original data pointed at real escape-room websites and about two thirds of those
# images are now gone.
# --------------------------------------------------------------------------------------------
$palettes = @{
    space      = @('#0b1d3a','#1b3b6f','#8fd0ff'); prison    = @('#2a2d34','#4a4e57','#c8ccd4')
    tomb       = @('#3d2c15','#6b4d24','#e8c37a'); asylum    = @('#16211f','#2c3d39','#9fb8b1')
    heist      = @('#1c1c22','#3a3a46','#d8b45a'); detective = @('#141a24','#2b3a4f','#c3d2e6')
    laboratory = @('#0f2620','#1d4a3c','#7dffc4'); pirate    = @('#12242c','#274652','#f0d9a8')
    submarine  = @('#04141c','#0d3244','#5fd4e8'); wizard    = @('#1d1230','#3b2560','#d9b3ff')
    bunker     = @('#22251c','#40452f','#c9d18a'); time      = @('#1a1626','#372c4d','#e0c9ff')
    museum     = @('#241f1a','#463c31','#e5d3b3'); outbreak  = @('#231212','#4d2020','#ff9d9d')
    fairytale  = @('#1b2436','#31456b','#ffd9e8'); steampunk = @('#241a12','#4a3524','#e0a45c')
}

function Get-Motif([string]$key, [string]$accent) {
    switch ($key) {
        'space'      { "<circle cx='300' cy='265' r='78' fill='none' stroke='$accent' stroke-width='9'/><ellipse cx='300' cy='265' rx='128' ry='40' fill='none' stroke='$accent' stroke-width='7' transform='rotate(-20 300 265)'/>" }
        'prison'     { "<g stroke='$accent' stroke-width='12' stroke-linecap='round'>" + (0..3 | ForEach-Object { "<line x1='$(240 + $_*40)' y1='185' x2='$(240 + $_*40)' y2='350'/>" }) + "</g><line x1='215' y1='185' x2='385' y2='185' stroke='$accent' stroke-width='12' stroke-linecap='round'/>" }
        'tomb'       { "<path d='M300 175 L395 350 L205 350 Z' fill='none' stroke='$accent' stroke-width='10' stroke-linejoin='round'/><path d='M300 245 L340 350 L260 350 Z' fill='$accent' opacity='0.5'/>" }
        'asylum'     { "<rect x='275' y='180' width='50' height='170' rx='8' fill='$accent'/><rect x='215' y='240' width='170' height='50' rx='8' fill='$accent'/>" }
        'heist'      { "<circle cx='300' cy='265' r='90' fill='none' stroke='$accent' stroke-width='10'/><circle cx='300' cy='265' r='26' fill='$accent'/><g stroke='$accent' stroke-width='9' stroke-linecap='round'><line x1='300' y1='150' x2='300' y2='185'/><line x1='300' y1='345' x2='300' y2='380'/><line x1='185' y1='265' x2='220' y2='265'/><line x1='380' y1='265' x2='415' y2='265'/></g>" }
        'detective'  { "<circle cx='283' cy='245' r='72' fill='none' stroke='$accent' stroke-width='11'/><line x1='334' y1='296' x2='395' y2='357' stroke='$accent' stroke-width='16' stroke-linecap='round'/>" }
        'laboratory' { "<path d='M270 170 L270 250 L215 350 Q210 365 228 365 L372 365 Q390 365 385 350 L330 250 L330 170 Z' fill='none' stroke='$accent' stroke-width='10' stroke-linejoin='round'/><line x1='258' y1='170' x2='342' y2='170' stroke='$accent' stroke-width='11' stroke-linecap='round'/><circle cx='285' cy='320' r='11' fill='$accent'/><circle cx='320' cy='340' r='8' fill='$accent'/>" }
        'pirate'     { "<circle cx='300' cy='195' r='24' fill='none' stroke='$accent' stroke-width='10'/><line x1='300' y1='219' x2='300' y2='360' stroke='$accent' stroke-width='11' stroke-linecap='round'/><line x1='248' y1='252' x2='352' y2='252' stroke='$accent' stroke-width='11' stroke-linecap='round'/><path d='M225 305 Q300 400 375 305' fill='none' stroke='$accent' stroke-width='11' stroke-linecap='round'/>" }
        'submarine'  { "<circle cx='300' cy='265' r='95' fill='none' stroke='$accent' stroke-width='12'/><circle cx='300' cy='265' r='62' fill='none' stroke='$accent' stroke-width='7' opacity='0.65'/>" + ((0..7 | ForEach-Object { $a=[math]::PI*2*$_/8; "<circle cx='$([int](300+112*[math]::Cos($a)))' cy='$([int](265+112*[math]::Sin($a)))' r='7' fill='$accent'/>" }) -join '') }
        'wizard'     { "<path d='M300 165 L322 235 L395 235 L336 278 L358 348 L300 305 L242 348 L264 278 L205 235 L278 235 Z' fill='none' stroke='$accent' stroke-width='9' stroke-linejoin='round'/>" }
        'bunker'     { "<circle cx='300' cy='265' r='26' fill='$accent'/>" + ((0..2 | ForEach-Object { "<path d='M300 265 L$([int](300+105*[math]::Cos([math]::PI*2*$_/3 - 1.05))) $([int](265+105*[math]::Sin([math]::PI*2*$_/3 - 1.05))) A105 105 0 0 1 $([int](300+105*[math]::Cos([math]::PI*2*$_/3 - 0.05))) $([int](265+105*[math]::Sin([math]::PI*2*$_/3 - 0.05))) Z' fill='$accent' opacity='0.85'/>" }) -join '') }
        'time'       { "<circle cx='300' cy='265' r='95' fill='none' stroke='$accent' stroke-width='10'/><line x1='300' y1='265' x2='300' y2='200' stroke='$accent' stroke-width='10' stroke-linecap='round'/><line x1='300' y1='265' x2='348' y2='295' stroke='$accent' stroke-width='10' stroke-linecap='round'/>" }
        'museum'     { "<path d='M195 210 L300 155 L405 210 Z' fill='none' stroke='$accent' stroke-width='10' stroke-linejoin='round'/><g stroke='$accent' stroke-width='12' stroke-linecap='round'>" + ((0..3 | ForEach-Object { "<line x1='$(232 + $_*45)' y1='232' x2='$(232 + $_*45)' y2='330'/>" }) -join '') + "</g><line x1='195' y1='352' x2='405' y2='352' stroke='$accent' stroke-width='12' stroke-linecap='round'/>" }
        'outbreak'   { "<circle cx='300' cy='265' r='30' fill='none' stroke='$accent' stroke-width='9'/>" + ((0..2 | ForEach-Object { $a=[math]::PI*2*$_/3 - 1.57; "<circle cx='$([int](300+72*[math]::Cos($a)))' cy='$([int](265+72*[math]::Sin($a)))' r='46' fill='none' stroke='$accent' stroke-width='9'/>" }) -join '') }
        'fairytale'  { "<path d='M215 350 L215 235 L245 235 L245 205 L275 205 L275 235 L325 235 L325 205 L355 205 L355 235 L385 235 L385 350 Z' fill='none' stroke='$accent' stroke-width='10' stroke-linejoin='round'/><path d='M278 350 L278 292 Q300 268 322 292 L322 350 Z' fill='$accent' opacity='0.6'/>" }
        'steampunk'  { "<circle cx='300' cy='265' r='58' fill='none' stroke='$accent' stroke-width='11'/><circle cx='300' cy='265' r='20' fill='$accent'/>" + ((0..7 | ForEach-Object { $a=[math]::PI*2*$_/8; "<line x1='$([int](300+58*[math]::Cos($a)))' y1='$([int](265+58*[math]::Sin($a)))' x2='$([int](300+92*[math]::Cos($a)))' y2='$([int](265+92*[math]::Sin($a)))' stroke='$accent' stroke-width='16' stroke-linecap='round'/>" }) -join '') }
        default      { "<circle cx='300' cy='265' r='80' fill='none' stroke='$accent' stroke-width='10'/>" }
    }
}

function Write-Poster($theme) {
    $p = $palettes[$theme.Key]
    $motif = (Get-Motif $theme.Key $p[2]) -join ''
    $title = $theme.Title.Replace("'", "&#39;").ToUpper()
    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 600" width="600" height="600" role="img" aria-label="$title">
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color="$($p[0])"/>
      <stop offset="100%" stop-color="$($p[1])"/>
    </linearGradient>
  </defs>
  <rect width="600" height="600" fill="url(#bg)"/>
  <rect x="26" y="26" width="548" height="548" fill="none" stroke="$($p[2])" stroke-width="3" opacity="0.35"/>
  $motif
  <text x="300" y="486" font-family="Segoe UI, Arial, sans-serif" font-size="40" font-weight="600"
        letter-spacing="5" fill="$($p[2])" text-anchor="middle">$title</text>
</svg>
"@
    $path = Join-Path $ArtDir ("{0}.svg" -f $theme.Key)
    [System.IO.File]::WriteAllText($path, $svg, (New-Object System.Text.UTF8Encoding($false)))
}

# --------------------------------------------------------------------------------------------
# Generate
# --------------------------------------------------------------------------------------------
if (-not (Test-Path $ArtDir)) { New-Item -ItemType Directory -Force $ArtDir | Out-Null }
Get-ChildItem $ArtDir -Filter *.svg -ErrorAction SilentlyContinue | Remove-Item -Force
foreach ($t in $themes) { Write-Poster $t }
Write-Host "Wrote $($themes.Count) posters to wwwroot/img/rooms" -ForegroundColor Green

# Countries, cities and streets, tracking the identity ids each insert will receive.
$countryLines = @(); $cityLines = @(); $streetLines = @()
$cityIdsByCountry = @{}; $streetIdsByCountry = @{}
$countryId = 0; $cityId = 0; $streetId = 0

foreach ($g in $geography) {
    $countryId++
    $countryLines += "insert into Countries (Name) values ('$(SqlEscape $g.Country)');"
    $cityIdsByCountry[$countryId] = @()
    foreach ($c in $g.Cities) {
        $cityId++
        $cityLines += "insert into Cities (Name) values ('$(SqlEscape $c)');"
        $cityIdsByCountry[$countryId] += $cityId
    }
    $streetIdsByCountry[$countryId] = @()
    foreach ($s in $g.Streets) {
        $streetId++
        $streetLines += "insert into Streets (Name) values ('$(SqlEscape $s)');"
        $streetIdsByCountry[$countryId] += $streetId
    }
}

# Companies
$companyLines = @(); $companyCount = 0
foreach ($prefix in $companyPrefixes) {
    $companyCount++
    $name = "$prefix $(Pick $companySuffixes)"
    $companyLines += "insert into Companies (Name) values ('$(SqlEscape $name)');"
}

# Flatten every theme into individual room concepts, so a country can be given a set with no
# repeats in it.
$concepts = @()
foreach ($t in $themes) {
    foreach ($conceptName in $t.Names) {
        $concepts += @{ Key = $t.Key; Fear = $t.Fear; Name = $conceptName }
    }
}
if ($concepts.Count -lt $RoomsPerCountry) {
    throw "Only $($concepts.Count) room concepts exist but $RoomsPerCountry are needed per country."
}

# Addresses and rooms: one address per room, always inside its own country.
$addressLines = @(); $roomLines = @(); $imageLines = @()
$addressId = 0; $roomId = 0

for ($cid = 1; $cid -le $geography.Count; $cid++) {
    # Sampled without replacement, so no country -- and therefore no page of results -- lists the
    # same room twice. A concept reappearing under a different country reads as a chain running
    # the same room at several locations, which is how the real ones operate.
    $selection = $concepts | Sort-Object { $random.Next() } | Select-Object -First $RoomsPerCountry

    foreach ($concept in $selection) {
        $addressId++
        $city   = Pick $cityIdsByCountry[$cid]
        $street = Pick $streetIdsByCountry[$cid]
        $house  = PickInt 1 180
        $addressLines += "insert into Addresses (HouseNumber, CityId, CountryId, StreetId) values ('$house', $city, $cid, $street);"

        $roomId++
        $name    = $concept.Name
        $blurb   = $roomBlurbs[$name]
        if (-not $blurb) { throw "No description written for room '$name'." }
        $company = PickInt 1 $companyCount
        $minP    = PickInt 2 3
        $maxP    = $minP + (PickInt 2 4)
        $minutes = (PickInt 3 6) * 15          # 45 to 90 minutes
        # TimeToPass is stored as text and read back with TimeSpan.Parse, so 90 minutes has to be
        # written as 01:30:00 rather than 00:90:00.
        $span    = [TimeSpan]::FromMinutes($minutes)
        $duration = '{0:00}:{1:00}:00' -f $span.Hours, $span.Minutes
        $rating  = PickInt 6 10                # out of 10
        $fear    = [Math]::Max(1, [Math]::Min(5, $concept.Fear + (PickInt -1 1)))
        $diff    = PickInt 2 5                 # out of 5
        $logo    = "/img/rooms/$($concept.Key).svg"

        $roomLines += ("insert into Rooms (Name, Description, TimeToPass, MinPlayers, MaxPlayers, Phone, Email, Rating, FearLevel, Difficulty, LogoPath, AddressId, CompanyId) values " +
                       "('$(SqlEscape $name)', '$(SqlEscape $blurb)', '$duration', $minP, $maxP, " +
                       "'+380 44 $((PickInt 100 999)) $((PickInt 1000 9999))', 'book$roomId@qwestrooms.example', " +
                       "$rating, $fear, $diff, '$logo', $addressId, $company);")

        $imageLines += "insert into Images (Path, RoomId) values ('$logo', $roomId);"
    }
}

function Save($name, $lines) {
    $path = Join-Path $DataDir $name
    [System.IO.File]::WriteAllLines($path, $lines, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host ("  {0,-16} {1,5} rows" -f $name, $lines.Count)
}

Write-Host "Writing SQL:" -ForegroundColor Cyan
Save 'Countries.sql' $countryLines
Save 'Cities.sql'    $cityLines
Save 'Streets.sql'   $streetLines
Save 'Companies.sql' $companyLines
Save 'Addresses.sql' $addressLines
Save 'Rooms.sql'     $roomLines
Save 'Images.sql'    $imageLines

Write-Host "`nDone. Now run:  .\tools\dev.ps1 reseed   then   .\tools\dev.ps1 run" -ForegroundColor Green
