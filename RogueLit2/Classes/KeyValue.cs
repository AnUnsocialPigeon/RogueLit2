namespace RogueLit2.Classes {
    internal class KeyValue {

        public static readonly int[] AllKeyValues = new int[] {
            32,37,38,39,40,65,68,83,87
        };
        public static String VerifyKey(int code) {

            // Top 10 most vile pieces of code to ever exist

            String key = "";
            if (code == 32) key = "[Space]";
            else if (code == 37) key = "[Left]";
            else if (code == 38) key = "[Up]";
            else if (code == 39) key = "[Right]";
            else if (code == 40) key = "[Down]";
            else if (code == 65) key = "a";
            else if (code == 68) key = "d";
            else if (code == 83) key = "s";
            else if (code == 87) key = "w";
            else key = "[" + code + "]";

            return key;
        }
    }
}
