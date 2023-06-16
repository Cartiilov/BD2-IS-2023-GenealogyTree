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
			Console.WriteLine("Ancestors of " + idc);
			getAncestors(idc);
			Console.WriteLine("\nChildren of " + idc);
			getChildren(idc);/*
			string sqlcmd = "update Person set );";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand(sqlcmd, cnn);

				try
				{
					cnn.Open();
					int changed = cmd.ExecuteNonQuery();

				}
				catch (SqlException ex)
				{
					Console.WriteLine(ex.Message);
				}
			}*/
		}
		
		public DataSet getAncestors(int id)
        {
			string gen = getHierarchyId(id);
			Console.WriteLine(gen);
			string sqlcmd = "EXEC GetAncestors @gen = '" + gen + "';";
			SqlDataAdapter adapter = null;
			DataSet dataSet = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				
				for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
				{
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
			string sqlcmd = "EXEC GetChildren @gen = '" + gen + "';";
			SqlDataAdapter adapter = null;
			DataSet dataSet = null;

			try
			{
				adapter = new SqlDataAdapter(sqlcmd, connectionString);
				dataSet = new DataSet();
				adapter.Fill(dataSet);
				for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
				{
					printPersonData2(dataSet.Tables[0].Rows[i]);
				}
				
			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.Message);
			}
			return dataSet;
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
/*
string sqlcmd = "EXEC GetAncestors @gen = '" + gen + "';";
for (int i = 0; i < dataSet.Tables[0].Rows.Count; ++i)
{
	printPersonData2(dataSet.Tables[0].Rows[i]);
}*/