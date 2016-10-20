namespace Logger.Net
{
    public class MyLog
    {
        private readonly string category;
        private readonly string user;

        public MyLog(string myCategory, string myUser)
        {
            category = myCategory;
            user = myUser;
            if (!Global.Log.IsCategoryExist(category))
            {
                Global.Log.WriteWarning("MyLog Object", "category does not exist in the configuration file");
            }
        }

        public void WriteError(string title, string description)
        {
            Global.Log.WriteError(title, description, user, category);
        }

        public void WriteWarning(string title, string description)
        {
            Global.Log.WriteWarning(title, description, user, category);
        }

        public void WriteInfo(string title, string description)
        {
            Global.Log.WriteInfo(title, description, user, category);
        }
    }
}