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
			while (true)
            {
				choicesMenu();
			}

		}

		public void choicesMenu()
        {
			Console.WriteLine("Choose from these options:");
			globalChoicesListing();
			Console.WriteLine("1. Add person");
			Console.WriteLine("2. Add parent to a person");
			Console.WriteLine("3. Get ancestors of a person");
			Console.WriteLine("4. Get descendants of a person");
			Console.WriteLine("5. Remove relationship between a parent and a child");
			Console.WriteLine("6. Remove a person");
			Console.WriteLine("7. Get person's data");

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
					Console.WriteLine("\n-> Get ancestors of a person");
					getAncestors();
					break;
				case "4":
					Console.WriteLine("\n-> Get descendants of a person");
					getDescendants();
					break;
				case "5":
					Console.WriteLine("\n-> Remove relationship between a parent and a child");
					removeRelationship();
					break;
				case "6":
					Console.WriteLine("\n-> Remove a person");
					break;
				case "7":
					Console.WriteLine("\n-> Get person's Data");
					break;
				default:
					Console.WriteLine("Incorrect input, try again!");
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

        public void getAncestors()
        {
			db.printAllPeople();
			int id = -2;
			while (true)
			{
				Console.WriteLine("\nPLease enter the id of the person whose ancestors you want to find:");
				id = getIdFromUser(id);
				if (db.isIdInDb(id)) break;
			}

			db.findAncestors(id);
		}

		public void getDescendants()
		{
			db.printAllPeople();
			int id = -2;
			while (true)
			{
				Console.WriteLine("\nPlease enter the id of the person whose ancestors you want to find:");
				id = getIdFromUser(id);
				if (db.isIdInDb(id)) break;
			}

			db.findDescendants(id);
		}
		public void getParents()
        {
			int id = -2;
            while (true)
            {
				Console.WriteLine("\nPLease enter the id of the person whose parents you want to find:");
				id = getIdFromUser(id);
				if (db.isIdInDb(id)) break;
			}

			//db.getParents(id);
		}

		public void getChildren()
		{
			int id = -2;
			while (true)
			{
				Console.WriteLine("\nPLease enter the id of the person whose children you want to find:");
				id = getIdFromUser(id);
				if (db.isIdInDb(id)) break;
			}
			//db.getChildren(id);
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
			string birth = getDate();
			Console.WriteLine("First name: " + fname);
			Console.WriteLine("Last name: " + lname);
			Console.WriteLine("Gender: " + gender);
			Console.WriteLine("Birthday: " + gender);
			db.addPerson(fname, lname, gender, birth);
		}

		string getDate()
		{ 
			string inp = "";
			Console.WriteLine("Enter date of birth in format yyyy-mm-dd");
			inp = Console.ReadLine();
			return inp;
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

		int getIdFromUser(int id)
        {
			string idstr = Console.ReadLine();
			optionToQuit(idstr);
			bool k = int.TryParse(idstr, out id);

			if (!k)
            {
				Console.WriteLine("Input must be numerical! Try again");
				id = getIdFromUser(id);
            }
			return id;
        }

		void modifyRelationship()
        {
			db.printAllPeople();
			int id1 = -1;
			int id2 = -1;
			bool getCorrectRel = false;
			string inp;
			
            while (!getCorrectRel)
            {
				Console.WriteLine("\nEnter the id number of the first person");
				inp = Console.ReadLine();
				optionToQuit(inp);
				id1 = Int32.Parse(inp);
				Console.WriteLine(id1);
                if (db.isIdInDb(id1))
                {
					Console.WriteLine("Is this the person you were thinking of? if yes type 'y', if no type 'n'");
					getCorrectRel = confirmation();
                }
            }
			getCorrectRel = false;
			Console.WriteLine("\nEnter the id number of the second person");
			while (!getCorrectRel)
			{
				inp = Console.ReadLine();
				optionToQuit(inp);
				id2 = Int32.Parse(inp);
				
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
						Console.WriteLine("\nEnter the id number of the second person");
					}

				}
			}
			string rel = establishRelationship(id1, id2);
			bool ok = db.modifyRelationship(id1, id2, rel);
            if (ok)
            {
				Console.WriteLine("Modyfying successful");
			}
            else
            {
				Console.WriteLine("Modyfying was not successful");
			}
			
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

        void removeRelationship()
        {
			db.printAllPeople();
			int id1 = -1;
			int id2 = -1;
			bool getCorrectRel = false;
			string inp;

			while (!getCorrectRel)
			{
				Console.WriteLine("\nEnter the id number of the parentn");
				inp = Console.ReadLine();
				optionToQuit(inp);
				id1 = Int32.Parse(inp);
				Console.WriteLine(id1);
				if (db.isIdInDb(id1))
				{
					Console.WriteLine("Is this the person you were thinking of? if yes type 'y', if no type 'n'");
					getCorrectRel = confirmation();
				}
			}
			getCorrectRel = false;
			Console.WriteLine("\nEnter the id number of the child");
			while (!getCorrectRel)
			{
				inp = Console.ReadLine();
				optionToQuit(inp);
				id2 = Int32.Parse(inp);

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
						Console.WriteLine("\nEnter the id number of the second person");
					}

				}
			}

			bool ok = db.removeRelationship(id1, id2);
			if (ok)
			{
				Console.WriteLine("Modyfying successful");
			}
			else
			{
				Console.WriteLine("Modyfying was not successful");
			}
		}

		bool confirmation()
        {
			
			string inp = Console.ReadLine();
			inp = inp.ToLower();
			optionToQuit(inp);
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

	}

}
