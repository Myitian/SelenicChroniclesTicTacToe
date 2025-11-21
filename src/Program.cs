namespace SelenicChroniclesTicTacToe;

internal class Program
{
    static void Main()
    {
        HashSet<ulong> playStates = [];
        HashSet<ulong> winStates = [];
        HashSet<ulong> drawnStates = [];
        Queue<ulong> queue = [];
        queue.Enqueue(0);
        while (queue.Count > 0)
        {
            ulong state = queue.Dequeue();
            int winner = TicTacToeState.CheckWinner(state);
            if (winner != 0)
            {
                winStates.Add(state);
                continue;
            }
            uint step = TicTacToeState.StepOf(state);
            state = TicTacToeState.WithStep(state, step);
            uint currentRound = (step >> 1) % 3;
            uint currentPlayer = (step & 1) + 1;
            ulong currentSign = (currentRound << 2) | currentPlayer;
            ulong mask = 0xF;
            for (int offset = 0; offset < 4 * 9; offset += 4, mask <<= 4)
            {
                if ((state & mask) == (currentSign << offset))
                {
                    state &= ~mask;
                    break;
                }
            }
            mask = 0xF;
            bool drawn = true;
            for (int offset = 0; offset < 4 * 9; offset += 4, mask <<= 4)
            {
                if ((state & mask) == 0)
                {
                    drawn = false;
                    ulong newStatus = state | (currentSign << offset);
                    ulong normalizedStatus = TicTacToeState.Normalize(newStatus);
                    if (playStates.Add(normalizedStatus))
                        queue.Enqueue(newStatus);
                }
            }
            if (drawn)
                drawnStates.Add(state);
        }

        Console.WriteLine("Count of Drawn States: " + drawnStates.Count); // Should be zero
        Console.WriteLine("Count of All Win States: " + winStates.Count);
        Console.WriteLine("Count of Player1 Win States: " + winStates.Count(it => (TicTacToeState.StepOf(it) & 1) == 0));
        Console.WriteLine("Count of Player2 Win States: " + winStates.Count(it => (TicTacToeState.StepOf(it) & 1) == 1));
        Console.WriteLine("Count of All Play States: " + playStates.Count);
    }
}
