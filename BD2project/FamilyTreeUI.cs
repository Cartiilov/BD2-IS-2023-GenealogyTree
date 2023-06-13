using System;

namespace FamilyTree
{
	public class GenealogyUI
	{
		private DBconnect db;


		public GenealogyUI()
		{
			db = new DBconnect("WINSERV01");
			Console.WriteLine("Welcome to your own family tree builder! Let's start with choosing frome these options:\n");
			Console.WriteLine("1. Add person\n");


			string inp = Console.ReadLine();

			switch (inp)
			{
				case "1":
					Console.WriteLine("Create a person\n");
					addPerson();
					break;
				default:
					break;

			}
		}

		void addPerson()
		{
			Console.WriteLine("CREATE A PERSON:\nENTER FIRST NAME:\n");
			string fname = Console.ReadLine();
			Console.WriteLine("First name: " + fname);
			Console.WriteLine("\nENTER LAST NAME:\n");
			string lname = Console.ReadLine();
			Console.WriteLine("Last name: " + lname);

		}

	}

}
