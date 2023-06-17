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

		public bool addPerson(string fname, string lname, string gender)
		{
            string sqlcmd = "insert into Person(firstname, lastname, gender) values ('" + fname + "', '" + lname + "', '" + gender + "')";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand(sqlcmd, cnn);

				try
				{
					cnn.Open();
					int changed = cmd.ExecuteNonQuery();
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
			Console.WriteLine("id:\t" + dataSet["id"].ToString());
			Console.WriteLine("first name:\t" + dataSet["firstname"].ToString());
			Console.WriteLine("lastname:\t" + dataSet["lastname"].ToString());
			Console.WriteLine("date of birth:\t" + dataSet["dateofbirth"].ToString());
			Console.WriteLine("gender:\t" + dataSet["gender"].ToString());
		}

		public void printPersonData(DataSet dataSet)
		{
			Console.WriteLine("id:\t" + dataSet.Tables[0].Rows[0]["id"].ToString());
			Console.WriteLine("first name:\t" + dataSet.Tables[0].Rows[0]["firstname"].ToString());
			Console.WriteLine("lastname:\t" + dataSet.Tables[0].Rows[0]["lastname"].ToString());
			Console.WriteLine("date of birth:\t" + dataSet.Tables[0].Rows[0]["dateofbirth"].ToString());
			Console.WriteLine("gender:\t" + dataSet.Tables[0].Rows[0]["gender"].ToString());
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



		public void modifyRelationship(int id1, int id2, string rel)
        {
			if(rel.Equals("c"))
            {
				addParent(id1, id2);
            }
            else
            {
				addParent(id2, id1);
			}
        }

		void addParent(int idc, int idp)
		{
			bool modifyGenOfPAn = true;
			Console.WriteLine("Ancestors of " + idp);
			DataSet parentAnc = getAncestors(idp);
			if(parentAnc.Tables[0].Rows.Count < 1)
            {
				modifyGenOfPAn = false;
            }
			string cgen = getHierarchyId(idc);
			string pgen = getHierarchyId(idp);
			string pgender = getGender(idp);

			if (cgen.Equals(""))
			{
				cgen = "/";
			}

			if (pgender.Equals("f"))
			{
				pgen = "/1";
			}
			else
			{
				pgen = "/2";
			}

			string newAncGen;
			int ancId;
			if(modifyGenOfPAn)
            {
				for(int i = 0; i  < parentAnc.Tables[0].Rows.Count; ++i)
                {
					newAncGen = pgen + parentAnc.Tables[0].Rows[i]["gener"];
					ancId = Convert.ToInt32(parentAnc.Tables[0].Rows[i]["id"]);
					modifyPersonGen(ancId, newAncGen);
				}
            }
			pgen += cgen;
			modifyPersonGen(idc, cgen);
			modifyPersonGen(idp, pgen);

		}

		bool modifyPersonGen(int id, string gen)
        {
			string sqlcmd = "update Person set gen ='" + gen + "' where id=" + id + ";";
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
		
		public DataSet getAncestors(int id)
        {
			string gen = getHierarchyId(id);
			DataSet dataSet = null;
			if (gen.Equals(""))
			{
				Console.WriteLine("This person doesn't have any relatives in the database");
				return dataSet;
			}
			string sqlcmd = "EXEC GetAncestors @gen = '" + gen + "';";
			SqlDataAdapter adapter = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				/*
				for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
				{
					printPersonData2(dataSet.Tables[0].Rows[i]);
				}*/
				return dataSet;

			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}

			return dataSet;
		}

		public DataSet getParents(int id)
		{
			string gen = getHierarchyId(id);
			DataSet dataSet = null;
			if (gen.Equals(""))
			{
				Console.WriteLine("This person doesn't have parents in the database");
				return dataSet;
			}
			string sqlcmd = "EXEC GET_PARENTS @gen = '" + gen + "';";
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


		public DataSet getDescendants(int id)
        {
			string gen = getHierarchyId(id);
			Console.WriteLine(gen);
			DataSet dataSet = null;
			if (gen.Equals(""))
            {
				Console.WriteLine("This person doesn't have any relatives in the database");
				return dataSet;
            }
			string sqlcmd = "EXEC GetChildren @gen = '" + gen + "';";
			SqlDataAdapter adapter = null;
			

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);/*
				for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
				{
					printPersonData2(dataSet.Tables[0].Rows[i]);
				}*/
				
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