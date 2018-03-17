namespace Pomutto
{
    public class Events
    {
        public class FSMEvent
        {
            public static string ClearBlocksCompleted = "CLEAR_BLOCKS_COMPLETED";
            public static string StopFallBlock = "STOP_FALL_BLOCK";
            public static string StopOneBlock = "STOP_ONE_BLOCK";
            public static string StopTwoBlock = "STOP_TWO_BLOCK";
            public static string NoFallBlocks = "NO_FALL_BLOCKS";
            public static string GameOver = "GAME_OVER";
            public static string BlockGroupReset = "BLOCK_GROUP_RESET";
        }
    }
}