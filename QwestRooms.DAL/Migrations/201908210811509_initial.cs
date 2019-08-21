namespace QwestRooms.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Adresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HouseNumber = c.String(),
                        City_Id = c.Int(),
                        Country_Id = c.Int(),
                        Street_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cities", t => t.City_Id)
                .ForeignKey("dbo.Countries", t => t.Country_Id)
                .ForeignKey("dbo.Streets", t => t.Street_Id)
                .Index(t => t.City_Id)
                .Index(t => t.Country_Id)
                .Index(t => t.Street_Id);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        TimeToPass = c.Time(nullable: false, precision: 7),
                        MinPayers = c.Int(nullable: false),
                        MaxPlayers = c.Int(nullable: false),
                        Phone = c.String(),
                        Email = c.String(),
                        Rating = c.Int(nullable: false),
                        FearLevel = c.Int(nullable: false),
                        Diffictly = c.Int(nullable: false),
                        LogoPath = c.String(),
                        Adress_Id = c.Int(),
                        Company_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Adresses", t => t.Adress_Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .Index(t => t.Adress_Id)
                .Index(t => t.Company_Id);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(),
                        Room_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Rooms", t => t.Room_Id)
                .Index(t => t.Room_Id);
            
            CreateTable(
                "dbo.Streets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Adresses", "Street_Id", "dbo.Streets");
            DropForeignKey("dbo.Images", "Room_Id", "dbo.Rooms");
            DropForeignKey("dbo.Rooms", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.Rooms", "Adress_Id", "dbo.Adresses");
            DropForeignKey("dbo.Adresses", "Country_Id", "dbo.Countries");
            DropForeignKey("dbo.Adresses", "City_Id", "dbo.Cities");
            DropIndex("dbo.Images", new[] { "Room_Id" });
            DropIndex("dbo.Rooms", new[] { "Company_Id" });
            DropIndex("dbo.Rooms", new[] { "Adress_Id" });
            DropIndex("dbo.Adresses", new[] { "Street_Id" });
            DropIndex("dbo.Adresses", new[] { "Country_Id" });
            DropIndex("dbo.Adresses", new[] { "City_Id" });
            DropTable("dbo.Streets");
            DropTable("dbo.Images");
            DropTable("dbo.Companies");
            DropTable("dbo.Rooms");
            DropTable("dbo.Countries");
            DropTable("dbo.Cities");
            DropTable("dbo.Adresses");
        }
    }
}
