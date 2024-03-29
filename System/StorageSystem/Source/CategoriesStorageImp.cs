﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CrawlerNameSpace.Utilities;

namespace CrawlerNameSpace.StorageSystem
{
    public class CategoriesStorageImp : CategoriesStorage
    {

        /**
         * returns the category list of the specified task
         */
        public List<Category> getCategories(String taskId)
        {
            List<Category> categoryList = new List<Category>();
            SqlConnection conn = null;
            SqlDataReader rdr = null;

            try
            {
                conn = new SqlConnection(SettingsReader.getConnectionString());

                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT CategoryID,CategoryName,Keywords,ParentCategory,ConfidenceLevel" +
                    " FROM Category WHERE TaskID=\'" + taskId + "\'", conn);

                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        int confidenceLevel = Convert.ToInt32(rdr["ConfidenceLevel"]);
                        List<String> dbkeywords = null, keywords = new List<string>();
                        if (!rdr["Keywords"].Equals(System.DBNull.Value))
                            dbkeywords = new List<string>(((String)rdr["Keywords"]).Split(';'));
                        foreach (string key in dbkeywords)
                        {
                            if (key.Length != 0) keywords.Add(key);
                        }
                        Category category = new Category(rdr["CategoryID"].ToString(), rdr["ParentCategory"].ToString(),
                            rdr["CategoryName"].ToString().Trim(), keywords, confidenceLevel);

                        categoryList.Add(category);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception Caught: " + e.Message);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null) conn.Close();
            }
            return categoryList;
        }

        /**
         * resets the predifined categories in the specified task
         * it will return the number of rows which has been removed due to this reset
         */
        public int resetCategories(String taskId)
        {
            SqlConnection conn = null;
            int rowsRemoved = 0;

            try
            {
                conn = new SqlConnection(SettingsReader.getConnectionString());

                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Category WHERE TaskID=\'" + taskId + "\'", conn);

                rowsRemoved = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception Caught: " + e.Message);
            }
            finally
            {
                if (conn != null) conn.Close();
            }
            return rowsRemoved;
        }

        /**
         * sets the categories for the specified task
         */
        public void setCategories(String taskId, List<Category> categoryList)
        {
            resetCategories(taskId);
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(SettingsReader.getConnectionString());

                conn.Open();
                foreach (Category category in categoryList)
                {
                    String keywords = "";
                    bool flag = true;
                    foreach (string token in category.getKeywordList())
                    {
                        if (flag == true)
                        {
                            keywords = token;
                            flag = false;
                        }
                        else keywords = keywords + ";" + token;
                    }

                    SqlCommand cmd = null;
                    Console.WriteLine(category.getParentName());
                    if ((category.getParentName() != null)&&(category.getParentName()!=""))
                    {
                        cmd = new SqlCommand("INSERT INTO Category (TaskID,CategoryName,Keywords,ParentCategory,ConfidenceLevel)"
                            + " Values (\'" + taskId + "\',\'" + category.getCatrgoryName() + "\',\'" + keywords + "\',\'"
                            + category.getParentName() + "\'," + category.getConfidenceLevel().ToString() + ")", conn);
                    }
                    else
                    {
                        cmd = new SqlCommand("INSERT INTO Category (TaskID,CategoryName,Keywords,ConfidenceLevel)"
                            + " Values (\'" + taskId + "\',\'" + category.getCatrgoryName() + "\',\'" + keywords + "\',"
                            + category.getConfidenceLevel().ToString() + ")", conn);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception Caught: " + e.Message);
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        /**
         * This method sets the given categoryID(of the parent) to be a parent
         * of the given other categoryID(of the son).
         */
        public void setParentToSon(String parentID, String sonID)
        {
            SqlConnection conn = new SqlConnection(SettingsReader.getConnectionString());
            try
            {
                conn.Open();
                String cmdtxt = "UPDATE Category SET ParentCategory=\'" + parentID + "\' WHERE CategoryID = \'" + sonID + "\'";
                if (parentID == "" || parentID == null)
                    cmdtxt = "UPDATE Category SET ParentCategory=NULL WHERE CategoryID = \'" + sonID + "\'";

                SqlCommand cmd = new SqlCommand(cmdtxt, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            } 
        }
    }
}
