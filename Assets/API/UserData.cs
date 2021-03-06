using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserData {
    public bool isNHH;
    public string userName;
    public int totalFish;

    public SortedDictionary<string, List<DailyRecord>> record;
    public SortedDictionary<string, List<Dictionary<string, int>>> taskProgress;
    public SortedDictionary<string, List<bool>> challengeProgress;
    public SortedDictionary<string, List<Submission>> submissionList;


    public string usersJournal;

    private static FileStream userDataJsonFile;

    public static List<bool> GetChallengeProgress(UserData user, Challenge challenge) {
        // Nếu user không có data về bất cứ challenge nào thì tạo data mới cho challenge
        if (user.challengeProgress == null)
            user.challengeProgress = new SortedDictionary<string, List<bool>>();

        /* Nếu user không có data nào về challenge đã chọn (tên là challenge.challengeName)
         * thì sẽ tạo data cho challenge đó
         */
        if (!user.challengeProgress.ContainsKey(challenge.challengeName)) {
            user.challengeProgress.Add(challenge.challengeName, new List<bool>());
            for (int i = 0; i < challenge.tasks.Count; i++)
                user.challengeProgress[challenge.challengeName].Add(false);
            SaveUserData(user);
        }
        /* Nếu như không có bất kì data nào về challengeProgress thì mặc định
         * tất cả progress là false
         */
        if (user.challengeProgress[challenge.challengeName].Count == 0) {
            for (int i = 0; i < challenge.tasks.Count; i++)
                user.challengeProgress[challenge.challengeName].Add(false);
            SaveUserData(user);
        }
        return user.challengeProgress[challenge.challengeName];
    }

    public static List<Dictionary<string, int>> GetTaskProgress(UserData user, Challenge challenge) {
        if (user.taskProgress == null)
            user.taskProgress = new SortedDictionary<string, List<Dictionary<string, int>>>();
        if (!user.taskProgress.ContainsKey(challenge.challengeName)) {
            user.taskProgress.Add(challenge.challengeName, new List<Dictionary<string, int>>());
            for (int i = 0; i < challenge.tasks.Count; i++)
                user.taskProgress[challenge.challengeName].Add(new Dictionary<string, int>());
            SaveUserData(user);
        }
        if (user.taskProgress[challenge.challengeName].Count == 0) {
            for (int i = 0; i < challenge.tasks.Count; i++)
                user.taskProgress[challenge.challengeName].Add(new Dictionary<string, int>());
            SaveUserData(user);
        }
        return user.taskProgress[challenge.challengeName];
    }

    [System.Obsolete]
    public static UserData LoadUserData() {
        string path;
        if (Application.platform == RuntimePlatform.Android)
            path = Application.persistentDataPath + "//Data//UserData.json";
        else
            path = Application.dataPath + "//Data//UserData.json";
        if (!File.Exists(path))
            SaveUserData(new UserData());
        FileStream userDataJsonFile = new FileStream(path, FileMode.Open);
        StreamReader reader = new StreamReader(userDataJsonFile);
        string json = reader.ReadToEnd();
        UserData loadDataJson = JsonConvert.DeserializeObject<UserData>(json);
        reader.Close();
        return loadDataJson;
    }
    public static void SaveUserData(UserData userdata) {
        string path;
        if (Application.platform == RuntimePlatform.Android)
            path = Application.persistentDataPath + "//Data//UserData.json";
        else
            path = Application.dataPath + "//Data//UserData.json";

        string json = JsonConvert.SerializeObject(userdata);
        json = FormatJson(json);

        File.WriteAllText(path, json);
    }

    [System.Obsolete]
    public void SaveUserJournal(string newUsersJournal) {
        UserData loadDataJson = LoadUserData(); // for saving json file
        loadDataJson.usersJournal = newUsersJournal;

        this.usersJournal = newUsersJournal; // for data object

        string json = JsonUtility.ToJson(loadDataJson);
        string path = Application.persistentDataPath + "//Data//UserData.json";
        userDataJsonFile = new FileStream(path, FileMode.Create);
        StreamWriter writer = new StreamWriter(userDataJsonFile);
        json = FormatJson(json);
        writer.WriteLine(json);

        writer.Close();
    }
    public static string FormatJson(string str) {
        str = (str ?? "").Replace("{}", @"\{\}").Replace("[]", @"\[\]");

        int INDENT_SIZE = 4;
        var inserts = new List<int[]>();
        bool quoted = false, escape = false;
        int depth = 0/*-1*/;

        for (int i = 0, N = str.Length; i < N; i++) {
            var chr = str[i];

            if (!escape && !quoted)
                switch (chr) {
                    case '{':
                    case '[':
                    inserts.Add(new[] { i, +1, 0, INDENT_SIZE * ++depth });
                    //int n = (i == 0 || "{[,".Contains(str[i - 1])) ? 0 : -1;
                    //inserts.Add(new[] { i, n, INDENT_SIZE * ++depth * -n, INDENT_SIZE - 1 });
                    break;
                    case ',':
                    inserts.Add(new[] { i, +1, 0, INDENT_SIZE * depth });
                    //inserts.Add(new[] { i, -1, INDENT_SIZE * depth, INDENT_SIZE - 1 });
                    break;
                    case '}':
                    case ']':
                    inserts.Add(new[] { i, -1, INDENT_SIZE * --depth, 0 });
                    //inserts.Add(new[] { i, -1, INDENT_SIZE * depth--, 0 });
                    break;
                    case ':':
                    inserts.Add(new[] { i, 0, 1, 1 });
                    break;
                }

            quoted = (chr == '"') ? !quoted : quoted;
            escape = (chr == '\\') ? !escape : false;
        }

        if (inserts.Count > 0) {
            var sb = new System.Text.StringBuilder(str.Length * 2);

            int lastIndex = 0;
            foreach (var insert in inserts) {
                int index = insert[0], before = insert[2], after = insert[3];
                bool nlBefore = (insert[1] == -1), nlAfter = (insert[1] == +1);

                sb.Append(str.Substring(lastIndex, index - lastIndex));

                if (nlBefore) sb.AppendLine();
                if (before > 0) sb.Append(new string(' ', before));

                sb.Append(str[index]);

                if (nlAfter) sb.AppendLine();
                if (after > 0) sb.Append(new string(' ', after));

                lastIndex = index + 1;
            }

            str = sb.ToString();
        }

        return str.Replace(@"\{\}", "{}").Replace(@"\[\]", "[]");
    }
}
