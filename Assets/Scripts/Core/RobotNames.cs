namespace Core.Core
{
    public static class RobotNames
    {
        private static readonly string[] names =
        {
            "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot",
            "Golf", "Hotel", "India", "Juliet", "Kilo", "Lima",
            "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo",
            "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "Xray",
            "Yankee", "Zulu"
        };

        public static string GetName(int index) // index는 0부터 시작
        {
            return names[index % names.Length]; // index가 names 배열 길이를 초과할 경우를 대비해 모듈로 연산 사용
        }
    }
}