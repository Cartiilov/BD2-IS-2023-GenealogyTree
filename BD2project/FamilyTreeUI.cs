using System;

namespace FamilyTree
{
	public class GenealogyUI
	{
		private DBconnect db;


		public GenealogyUI()
		{
			db = new DBconnect("WINSERV01");
			Console.WriteLine("Welcome to your own family tree builder!\n");
			choicesMenu();
		}

		public void choicesMenu()
        {
			Console.WriteLine("Choose from these options:");
			globalChoicesListing();
			Console.WriteLine("1. Add person");
			Console.WriteLine("2. Modify relationship between two people");
			Console.WriteLine("3. Get parents of a person");
			Console.WriteLine("4. Get siblings of a person");
			Console.WriteLine("5. Get children of a person");
			Console.WriteLine("6. Remove a person");

			Console.WriteLine("Now enter the number of the option you want to choose:");
			string inp = Console.ReadLine();
			optionToQuit(inp);
			
			switch (inp)
			{
				case "1":
					Console.WriteLine("\n-> Add a person");
					addPerson(false);
					break;
				case "2":
					Console.WriteLine("\n-> Modify relationship between two people");
					modifyRelationship();
					break;
				case "3":
					Console.WriteLine("\n-> Get parents of a person");
					break;
				case "4":
					break;
				default:
					Console.WriteLine("Incorrect input, try again!");
					choicesMenu();
					break;

			}
		}

		public void globalChoicesListing()
        {
			Console.WriteLine("To quit the program input 'x' whenever you are asked for input");
			Console.WriteLine("To see the main menu input 'm' whenever you are asked for input\n");
		}

		public void optionToQuit(string opt)
        {
			switch (opt)
			{
				case "x":
					Console.WriteLine("Goodbye!");
					Environment.Exit(0);
					break;
				case "m":
					choicesMenu();
					break;
				default:
					break;
			}
        }

		void addPerson(bool relMod)
		{
			Console.WriteLine("CREATE A PERSON:\nENTER FIRST NAME:\n");
			string fname = Console.ReadLine();
			optionToQuit(fname);
			Console.WriteLine("\nENTER LAST NAME:\n");
			string lname = Console.ReadLine();
			optionToQuit(lname);
			string gender = inputGender();
			Console.WriteLine("First name: " + fname);
			Console.WriteLine("Last name: " + lname);
			Console.WriteLine("Gender: " + gender);
			db.addPerson(fname, lname, gender);
			choicesMenu();
		}
		
		void printRelationshipTypes()
        {
			Console.WriteLine("'p' - a is a parent of b");
			Console.WriteLine("'c' - a is a child of b");
		}
		string inputRelationship()
        {
			string rel = "";
			bool getCorrectRel = false;
			while (!getCorrectRel)
			{
				Console.WriteLine("\nENTER TYPE OF RELATIONSHIP:\n");
				printRelationshipTypes();
				rel = Console.ReadLine();
				optionToQuit(rel);
                switch (rel)
                {
					case "p":
						getCorrectRel = true;
						break;
					case "c":
						getCorrectRel = true;
						break;
					default:
						Console.WriteLine("Incorrect input, try again!");
						break;
				}
			}
			return rel;
		}

		string inputGender()
		{
			string g = "";
			bool getCorrect = false;
			while (!getCorrect)
			{
				Console.WriteLine("\nENTER GENDER\t f - female, m - male:\n");
				g = Console.ReadLine();
				g = g.ToLower();
				optionToQuit(g);
				switch (g)
				{
					case "m":
						getCorrect = true;
						break;
					case "f":
						getCorrect = true;
						break;
					default:
						Console.WriteLine("Incorrect input, try again!");
						break;
				}
			}
			return g;
		}

		void modifyRelationship()
        {
			int id1 = -1;
			int id2 = -1;
			bool getCorrectRel = false;
			Console.WriteLine("Enter the id number of the first person");
            while (!getCorrectRel)
            {
				id1 = Int32.Parse(Console.ReadLine());
				Console.WriteLine(id1);
                if (db.isIdInDb(id1))
                {
					Console.WriteLine("Is this the person you were thinking of? if yes type 'y', if no type 'n'");
					getCorrectRel = confirmation();
                }
            }
			getCorrectRel = false;
			Console.WriteLine("Enter the id number of the second person");
			while (!getCorrectRel)
			{
				id2 = Int32.Parse(Console.ReadLine());
				Console.WriteLine(id2);
				if (db.isIdInDb(id2))
				{
					if (id1 != id2)
					{
						Console.WriteLine("Is this the person you were thinking of? if yes type 'y', if no type 'n'");
						getCorrectRel = confirmation();
					}
                    else
                    {
						Console.WriteLine("You can modify a relatinship only between two different people! Try again!");
					}

				}
			}
			string rel = establishRelationship(id1, id2);
			db.modifyRelationship(id1, id2, rel);
		}

		string establishRelationship(int id1, int id2)
        {
			string person1 = db.getName(id1);
			string person2 = db.getName(id2);

			Console.WriteLine("Who is " + person1 + " to " + person2 + "?");
			printRelationshipTypes();
			string inp = inputRelationship();
			return inp;
        }

		bool confirmation()
        {
			
			string inp = Console.ReadLine();

			switch(inp)
            {
				case "y":
					return true;
				case "n":
					return false;
				default:
					Console.WriteLine("Incorrect input, try again!");
					break;
            }
			return false;
		}
		void getParents()
        {
			Console.WriteLine();
        }

	}

}
