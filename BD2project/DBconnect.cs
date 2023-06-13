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
			connectionString = string.Format(@"Data Source=" + srvName + " ; Initial Catalog=BD2; INTEGRATED SECURIT=SSPI;");

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
				return false;
			}
			return false;
		}

	}
}

