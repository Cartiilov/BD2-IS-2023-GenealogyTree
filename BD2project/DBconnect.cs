using System;
using System.Data;
using System.Data.SqlClient;


namespace FamilyTree
{
    public class DBconnect
	{
		private string connectionString;

		public DBconnect(String srvName)
		{
			connectionString = string.Format(@"Data Source=" + srvName + " ; Initial Catalog=bd2project; User ID=sa;Password=123");
		}

		public bool addPerson(string fname, string lname, string gender, string birth)
		{
            string sqlcmd = "EXEC AddPerson @fname='" + fname + "', @lname='" + lname + "', @dob='" + birth + "', @gender='" + gender + "';";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand(sqlcmd, cnn);

				try
				{
					cnn.Open();
					int changed = cmd.ExecuteNonQuery();
                    if (changed > 0)
                    {
						Console.WriteLine("Successfully added a person\n");
                    }
					return changed > 0;
				}
				catch (SqlException ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			return false;
		}

		public void printPersonData2(DataRow dataSet)
        {
			Console.WriteLine(dataSet["id"].ToString() + ". " + dataSet["firstname"].ToString() + " " + dataSet["lastname"].ToString());
			Console.WriteLine();
		}

		public void printPersonData(DataSet dataSet)
		{
			Console.WriteLine(dataSet.Tables[0].Rows[0]["id"].ToString() + ". " + dataSet.Tables[0].Rows[0]["firstname"].ToString() + " " + dataSet.Tables[0].Rows[0]["lastname"].ToString() + " (" + dataSet.Tables[0].Rows[0]["gender"].ToString() + ")");
			Console.WriteLine("date of birth:\t" + dataSet.Tables[0].Rows[0]["dateofbirth"].ToString() + "\n");
		}

		public void printAncestorsWithGens(DataRow dt)
        {
			string gender = dt["gender"].ToString();
			string rel = determineRelationshipAnc(Convert.ToInt32(dt["n"]), gender);
			Console.WriteLine(rel + " " + dt["id"].ToString() + ". " + dt["firstname"].ToString() + " " + dt["lastname"].ToString() + " (" + gender + ")");
			Console.WriteLine();
		}

		public void printDescendantsWithGens(DataRow dt)
		{
			string gender = dt["gender"].ToString();
			//string rel = determineRelationshipAnc(Convert.ToInt32(dt["n"]), gender);
			Console.WriteLine(dt["id"].ToString() + ". " + dt["firstname"].ToString() + " " + dt["lastname"].ToString() + " (" + gender + ")");
			Console.WriteLine();
		}

		string determineRelationshipAnc(int num, string gender)
        {
			string rel;
            if (gender.Equals("f"))
            {
                switch (num)
                {
					case 2:
						rel = "Mother";
						break;
					case 3:
						rel = "Grandmother";
						break;
					case 4:
						rel = "Great grandmother";
						break;
					default:
						rel = "Great grandmother x" + num;
						break;
				}
            }
            else
            {
				switch (num)
				{
					case 2:
						rel = "Father";
						break;
					case 3:
						rel = "Grandfather";
						break;
					case 4:
						rel = "Great grandfather";
						break;
					default:
						rel = "Great grandfather x" + num;
						break;
				}
			}
			return rel;
        }


		public void printAllPeople()
		{
			DataSet ppl = getEveryone();
			for (int i = 0; i < ppl.Tables[0].Rows.Count; ++i)
			{
				printPersonData2(ppl.Tables[0].Rows[i]);
			}
		}

		public DataSet getEveryone()
        {
			string sqlcmd = "EXEC GetAllPeople";
			SqlDataAdapter adapter = null;
			DataSet dataSet = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;
		}



		public bool isIdInDb(int id)
			{
				string sqlcmd = "select id, firstname, lastname, dateofbirth, gender from Person where id=" + id + ";";
				SqlDataAdapter adapter = null;
				DataSet dataSet = null;

				try
				{
					adapter = new SqlDataAdapter(sqlcmd, connectionString);
					dataSet = new DataSet();
					adapter.Fill(dataSet);
					if(dataSet.Tables[0].Rows.Count < 1)
					{
						Console.WriteLine("A person that has id=" + id + " does not exist in this database\nEnter the id again:w");
						return false;
					}
					printPersonData(dataSet);
					return true;

				}
				catch (SqlException ex)
				{
					Console.WriteLine(ex.Message);
				}

				return false;
			}

		void removeRelationship(int id1, int id2)
        {

        }

		public DataSet getPersonData(int id)
		{
			string sqlcmd = "select id, firstname, lastname, dateofbirth, gender from Person where id=" + id + ";";
			SqlDataAdapter adapter = null;
			DataSet dataSet = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				printPersonData(dataSet);
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;

		}

		public string getName(int id)
		{
			DataSet person = getPersonData(id);
			string ret = person.Tables[0].Rows[0]["firstname"] + " " + person.Tables[0].Rows[0]["lastname"];
			return ret;

		}



		public bool modifyRelationship(int id1, int id2, string rel)
        {
			if(rel.Equals("c"))
            {
				return addParent(id1, id2);
            }
            else
            {
				return addParent(id2, id1);
			}
        }

		bool addParent(int idc, int idp)
		{
			string sqlcmd = "EXEC AddParent @idp=" + idp + ", @idc=" + idc + ";";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand(sqlcmd, cnn);

				try
				{
					cnn.Open();
					int changed = cmd.ExecuteNonQuery();

					if (changed > 0) return true;
				}
				catch (SqlException ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			return false;
		}


		
		bool modifyMid(int id, int mid)
        {
			string sqlcmd = "update Person set mid ='" + mid  + "' where id=" + id + ";";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand(sqlcmd, cnn);

				try
				{
					cnn.Open();
					int changed = cmd.ExecuteNonQuery();

					if (changed > 0) return true;
				}
				catch (SqlException ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			return false;
		}

		public void findAncestors(int id)
        {
			DataSet dt = getAncestors(id);

			for (int i = 0; i < dt.Tables[0].Rows.Count; ++i)
			{
				printAncestorsWithGens(dt.Tables[0].Rows[i]);
			}
		}

		public void findDescendants(int id)
		{
			DataSet dt = getDescendants(id);

			for (int i = 0; i < dt.Tables[0].Rows.Count; ++i)
			{
				printDescendantsWithGens(dt.Tables[0].Rows[i]);
			}
		}
		DataSet getAncestors(int id)
        {
			DataSet dataSet = null;
			string sqlcmd = "EXEC GetAncestors @id=" + id +";";
			SqlDataAdapter adapter = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;
		}
		/*
		public DataSet getParents(int id)
		{
			string gen = getHierarchyId(id);
			DataSet dataSet = null;
			if (gen.Equals(""))
			{
				Console.WriteLine("This person doesn't have parents in the database");
				return dataSet;
			}
			string sqlcmd = "EXEC GetParents @gen = '" + gen + "';";
			SqlDataAdapter adapter = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				
				for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
				{
                    if (dataSet.Tables[0].Rows[0]["gender"].ToString().Equals("f"))
                    {
						Console.WriteLine("Mother:");
                    }
                    else
                    {
						Console.WriteLine("Father:");

					}
					printPersonData2(dataSet.Tables[0].Rows[i]);
				}
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;
		}

		public DataSet getChildren(int id)
		{
			string gen = getHierarchyId(id);
			Console.WriteLine(gen);
			DataSet dataSet = null;
			if (gen.Equals(""))
			{
				Console.WriteLine("This person doesn't have any children in the database");
				return dataSet;
			}
			string sqlcmd = "EXEC GET_CHILDREN @gen = '" + gen + "';";
			SqlDataAdapter adapter = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);

				for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
				{
					if (dataSet.Tables[0].Rows[0]["gender"].ToString().Equals("f"))
					{
						Console.WriteLine("Daughter:");
					}
					else
					{
						Console.WriteLine("Son:");

					}
					printPersonData2(dataSet.Tables[0].Rows[i]);
				}
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;
		}
		*/


		public DataSet getDescendants(int id)
        {
			DataSet dataSet = null;
			string sqlcmd = "EXEC GetDescendants @id=" + id + ";";
			SqlDataAdapter adapter = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;
		}

		string getGender(int id)
        {
			string gender = "";
			string sqlcmd = "Select gender from person where id=" + id + ";";
			SqlDataAdapter adapter = null;
			DataSet dataSet = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);

				gender = dataSet.Tables[0].Rows[0]["gender"].ToString();
				return gender;
			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}
			return gender;
		}
			
		string getHierarchyId(int id)
        {
			string gen = "";
			string sqlcmd = "EXEC GetGen @id=" + id + ";";
			SqlDataAdapter adapter = null;
			DataSet dataSet = null;

			try
			{
				
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);

				gen = dataSet.Tables[0].Rows[0]["gen"].ToString();
				return gen;
			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}
			return gen;
		}
		


	}
}