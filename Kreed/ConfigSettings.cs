using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Windows.Forms;



namespace ConfigurationSettings
{

    public static class StringExtensions
    {
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
    }


    public static class Functions
    {

    }


    public class MyApp
    {

        public MyApp()
        {

        }

        private static DataTable GetAppInfo()
        {
            try
            {


                MyApp.AppID = 1;
                MyApp.ExpiryDate = new DateTime(3000, 01, 01);
                MyApp.LicenseDaysLeft = (MyApp.ExpiryDate.Date - DateTime.Now.Date).Days;


                DataTable dt = new DataTable();

#if !DEBUG
                string sSQL = $"EXEC sp_AppConfig_InsUpd {MyApp.AppID}, '{MyApp.CompanyName}', '{MyApp.Name}', '{MyApp.ComputerName}', '{MyApp.Version}'";
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    string sCon = "Data Source=41.76.210.79\\sqlexpress;Initial Catalog=createch;Persist Security Info=True;User ID=sa;Password=sql2008;MultipleActiveResultSets=true";
                    da.SelectCommand = new SqlCommand(sSQL, new SqlConnection(sCon));
                    dt.Clear();
                    da.Fill(dt);
                }
                if (dt.Rows.Count > 0)
                {

                    MyApp.AppID = dt.Rows[0].Field<int>("AutoIDX");
                    MyApp.ExpiryDate = dt.Rows[0].Field<DateTime>("dtExpiry");
                    MyApp.LicenseDaysLeft = (MyApp.ExpiryDate.Date - DateTime.Now.Date).Days;
                }
                    
#endif
                    return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error obtaining App info. \n" + ex.ToString());
                return null;
            }
        }

        private static ConfigSettings settings = new ConfigSettings();
        static private Configuration PrepEnvironment()
        {
            try
            {

                System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
                customCulture.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
                customCulture.DateTimeFormat.LongDatePattern = "yyyy MMMM dd";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;


                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error setting environmentvariables: {e.ToString()}");
                throw;
            }

        }


        public static string ExePath = Application.ExecutablePath;
        public static string Path = System.IO.Path.GetDirectoryName(ExePath);
        public static Configuration configFile = PrepEnvironment();
        public static string CompanyName = "Lionels Vet";
        public static string Name = "EDI Import";
        public static string ComputerName = Environment.MachineName;
        public static Cons Cons = (new ConfigSettings()).ConApperances;
        
        public static int AppID;
        public static DateTime ExpiryDate;
        public static int LicenseDaysLeft = 0;
        private static DataTable dtAppInfo = GetAppInfo();
        
#if DEBUG
        // add debug mode code goes here
        public static Con Evo = Cons["EvoKreed"];

#else
        // add release mode code goes here
        public static Con Evo = Cons["EvoLive"];
#endif
        
        public static string Version
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        return "Debug Mode";
                    }
                    else
                    {
                        try
                        {
                            return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

                        }
                        catch (Exception err)
                        {
                            return err.ToString();
                        }
                    };
                }
                else
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                    return fvi.FileVersion;
                }
            }
        }
    }
    

    /// <summary>
    /// Node
    /// </summary>
    public class Con : ConfigurationElement
    {

        [ConfigurationProperty("Name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["Name"]; }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("Username", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Username
        {
            get { return (string)base["Username"]; }
            set { base["Username"] = value; }
        }
        [ConfigurationProperty("Password", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Password
        {
            get { return (string)base["Password"]; }
            set { base["Password"] = value; }
        }
        [ConfigurationProperty("Server", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Server
        {
            get { return (string)base["Server"]; }
            set { base["Server"] = value; }
        }
        [ConfigurationProperty("Database", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Database
        {
            get { return (string)base["Database"]; }
            set { base["Database"] = value; }
        }

        [ConfigurationProperty("ConnectionTimeOut", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ConnectionTimeOut
        {
            get { return (string)base["ConnectionTimeOut"]; }
            set { base["ConnectionTimeOut"] = value; }
        }

        [ConfigurationProperty("CommandTimeOut", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string CommandTimeOut
        {
            get { return (string)base["CommandTimeOut"]; }
        }


        public SqlConnection SQLCon { get; set; }
        public SqlTransaction SQLTrans { get; set; }

        // some methods for querying a database
        public bool ExecSQL(string sSQL)
        {
            try
            {
                if (this.SQLCon != null)
                {
                    if (this.SQLCon.State != ConnectionState.Open)
                    {
                        this.SQLCon.Open();
                    }

                    using (this.SQLTrans = this.SQLCon.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(sSQL, this.SQLCon))
                            {
                                cmd.CommandTimeout = Convert.ToInt32(this.CommandTimeOut);
                                cmd.Transaction = this.SQLTrans;
                                cmd.ExecuteNonQuery();
                                this.SQLTrans.Commit();
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            this.SQLTrans.Rollback();
                            MessageBox.Show("An error ocurred: " + e.Message + " -> SQL:" + sSQL);
                            return false;
                        }

                    }


                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error ocurred: " + e.Message + " -> SQL:" + sSQL);

                return false;
            }
        }

        public bool ExecSQL(string sSQL, ref DataTable dt)
        {
            try
            {
                if (this.SQLCon != null)
                {
                    if (this.SQLCon.State != ConnectionState.Open)
                    {
                        this.SQLCon.Open();
                    }

                    using (this.SQLTrans = this.SQLCon.BeginTransaction())
                    {
                        SqlDataAdapter da = new SqlDataAdapter(sSQL, this.SQLCon);
                        try
                        {
                            if (dt == null) dt = new DataTable();
                            da.SelectCommand.CommandTimeout = Convert.ToInt32(this.CommandTimeOut);
                            da.SelectCommand.Transaction = this.SQLTrans;
                            dt.Clear();
                            da.Fill(dt);
                            if (this.SQLTrans != null) this.SQLTrans.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            this.SQLTrans.Rollback();
                            MessageBox.Show("An error ocurred: " + e.Message + " -> SQL:" + sSQL);
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error ocurred: " + e.Message + " -> SQL:" + sSQL);
                return false;
            }
        }

        public class Params
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }
        public bool ExecSQL(string sSQL, ref DataTable dt, List<Params> parms)
        {
            try
            {
                if (this.SQLCon != null)
                {
                    if (this.SQLCon.State != ConnectionState.Open)
                    {
                        this.SQLCon.Open();
                    }
                    using (this.SQLTrans = this.SQLCon.BeginTransaction())
                    {
                        SqlDataAdapter da = new SqlDataAdapter(sSQL, this.SQLCon);
                        try
                        {
                            if (dt == null) dt = new DataTable();
                            parms.ForEach(o => da.SelectCommand.Parameters.AddWithValue("@" + o.Name, o.Value));
                            da.SelectCommand.CommandTimeout = Convert.ToInt32(this.CommandTimeOut);
                            da.SelectCommand.Transaction = this.SQLTrans;

                            dt.Clear();
                            da.Fill(dt);
                            this.SQLTrans.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            this.SQLTrans.Rollback();
                            MessageBox.Show("An error ocurred: " + e.Message + " -> SQL:" + sSQL);
                            return false;
                        }
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error ocurred: " + e.Message + " -> SQL:" + sSQL);
                return false;
            }
        }
        public void Open()
        {
            this.SQLCon = new SqlConnection();
            try
            {
                this.SQLCon.ConnectionString = "Data Source=" + this.Server + 
                        ";Initial Catalog=" + this.Database + 
                        ";Persist Security Info=True;User ID=" +
                        this.Username + ";Password=" +
                        this.Password + ";MultipleActiveResultSets=true";

                if (ConnectionState.Open != this.SQLCon.State)
                { this.SQLCon.Open(); }
            }
            catch (Exception err)
            {
                MessageBox.Show($"Connection Error: {err.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }


    /// <summary>
    /// Collection
    /// 
    /// </summary>
    [ConfigurationCollection(typeof(Con))]
    public class Cons : ConfigurationElementCollection
    {
        internal const string PropertyName = "Con";

        public override ConfigurationElementCollectionType CollectionType { get { return ConfigurationElementCollectionType.BasicMapAlternate; } }
        protected override string ElementName { get { return PropertyName; } }
        protected override bool IsElementName(string elementName) { return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase); }
        public override bool IsReadOnly() { return false; }
        protected override ConfigurationElement CreateNewElement() { return new Con(); }
        protected override object GetElementKey(ConfigurationElement element) { return ((Con)(element)).Name; }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }
        public void Add(Con con)
        {
            BaseAdd(con);
        }
        public Con this[int idx]
        {
            get
            {

                Con ThisCon = (Con)BaseGet(idx);
                ThisCon.SQLCon = new SqlConnection();
                try
                {
                    ThisCon.SQLCon.ConnectionString = "Data Source=" + ThisCon.Server + ";Initial Catalog=" + ThisCon.Database + ";Persist Security Info=True;User ID=" + ThisCon.Username + ";Password=" + ThisCon.Password + ";MultipleActiveResultSets=true";

                    if (ConnectionState.Open != ThisCon.SQLCon.State)
                    { ThisCon.SQLCon.Open(); }
                }
                catch (Exception err)
                {
                    //MyApp.Log.WriteEntry(err.ToString() + " - " + ThisCon.SQLCon.ConnectionString + "\n" + err.ToString(), System.Diagnostics.EventLogEntryType.Error);
                }
                return ThisCon;
            }
            set
            {
                if (BaseGet(idx) != null)
                {
                    BaseRemoveAt(idx);
                }
                BaseAdd(idx, value);
            }
        }

        new public Con this[string Name]
        {
            get
            {
                Con ThisCon = (Con)BaseGet(Name);
                ThisCon.SQLCon = new SqlConnection();
                try
                {
                    ThisCon.SQLCon.ConnectionString = "Persist Security Info=True" +
                                            ";Password=" + ThisCon.Password +
                                            ";User ID=" + ThisCon.Username +
                                            ";Data Source=" + ThisCon.Server +
                                            ";Initial Catalog=" + ThisCon.Database + ";MultipleActiveResultSets=true";
                    if (ConnectionState.Open != ThisCon.SQLCon.State)
                    { ThisCon.SQLCon.Open(); }
                }
                catch (Exception err)
                {
                    MessageBox.Show("Error: " + err.ToString());
                }
                return ThisCon;
            }
        }
    }


    /// <summary>
    /// ???
    /// </summary>
    public class ConnectionSection : ConfigurationSection
    {
        [ConfigurationProperty("Cons")]
        public Cons ConElement
        {
            get { return ((Cons)(base["Cons"])); }
            set { base["Cons"] = value; }
        }
    }



    /// <summary>
    /// ???
    /// </summary>
    public class ConfigSettings
    {
        public ConnectionSection ConAppearanceConfiguration
        {
            get
            {
                return (ConnectionSection)ConfigurationManager.GetSection("ConSection");
            }
        }

        public Cons ConApperances
        {
            get
            {
                return this.ConAppearanceConfiguration.ConElement;
            }
        }

        public IEnumerable<Con> ConElements
        {
            get
            {
                foreach (Con selement in this.ConApperances)
                {
                    if (selement != null)
                        yield return selement;
                }
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    ///         

        

}
