using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTree
{
	public class DBconnect
	{
		private string connectionString;

		public DBconnect(String srvName)
		{
			connectionString = string.Format(@"Data Source=" + srvName + " ; Initial Catalog=bd2project; User ID=sa;Password=123");
		}

		public bool addPerson(string fname, string lname)
		{
            string sqlcmd = "insert into Person(firstname, lastname, gen) values ('" + fname + "', '" + lname + "', '/');";
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

        }

	}
}

