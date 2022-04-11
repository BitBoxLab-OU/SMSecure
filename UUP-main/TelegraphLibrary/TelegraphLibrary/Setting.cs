using System;

namespace EncryptedMessaging
{
    public class Setting
    {

        public static int MessagePageCount = 20;
        public Setting(Context context)
        {
            _context = context;
            Load();
        }
        private readonly Context _context;

        private int _postPersistenceDays;

        /// <summary>
        /// Number of days messages must be kept in memory before being automatically deleted
        /// </summary>
        public int PostPersistenceDays
        {
            get => _postPersistenceDays;
            set { _postPersistenceDays = value; _context.SecureStorage.Values.Set("PostPersistenceDays", value); }
        }

        private int _keepPosts;
        /// <summary>
        /// Number of messages to be saved for each chat
        /// </summary>
        public int KeepPost
        {
            get => _keepPosts;
            set { _keepPosts = value; _context.SecureStorage.Values.Set("KeepPosts", value); }
        }

        private void Load()
        {
            _postPersistenceDays = _context.SecureStorage.Values.Get("PostPersistenceDays", 365);
            _keepPosts = _context.SecureStorage.Values.Get("KeepPosts", 1000);
        }

    }
}