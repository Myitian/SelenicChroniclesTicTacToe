using System.Numerics;

namespace SelenicChroniclesTicTacToe;

// Represent Tic-Tac-Toe board in a UInt64
// [63-36  ][35-32]|[31-28][27-24][23-20][19-16]|[15-12][11-08][07-04][03-00]
// [FFFFFFF][F    ]|[F    ][F    ][F    ][F    ]|[F    ][F    ][F    ][F    ]
// [step   ][s:1,1]|[s:0,0][s:0,1][s:0,2][s:1,2]|[s:2,2][s:2,1][s:2,0][s:1,0]
public static class TicTacToeState
{
    public static ulong Normalize(ulong state)
    {
        return (state & 0x0000_000F_0000_0000) | NormalizeMirror((uint)state);
    }
    public static uint NormalizeRotation(uint state)
    {
        uint rotate0 = state;
        uint rotate1 = BitOperations.RotateRight(rotate0, 8);
        uint rotate2 = BitOperations.RotateRight(rotate0, 16);
        uint rotate3 = BitOperations.RotateRight(rotate0, 24);
        return Math.Min(Math.Min(rotate0, rotate1), Math.Min(rotate2, rotate3));
    }
    public static uint NormalizeMirror(uint state)
    {
        uint mirror0 = (state & 0xF000F000)  //        [.12]
            | ((state & 0x0F00_0000) >> 24)  // [1>7]  [7.3]
            | ((state & 0x00F0_0000) >> 16)  // [2>6]  [65.]
            | ((state & 0x000F_0000) >> 8)   // [3>5]    V
            | ((state & 0x0000_0F00) << 8)   // [5>3]  [.76]
            | ((state & 0x0000_00F0) << 16)  // [6>2]  [1.5]
            | ((state & 0x0000_000F) << 24); // [7>1]  [23.]

        uint mirror1 = (state & 0x0F000F00)  //        [0.2]
            | ((state & 0xF000_0000) >> 8)   // [0>2]  [7.3]
            | ((state & 0x00F0_0000) << 8)   // [2>0]  [6.4]
            | ((state & 0x000F_0000) >> 16)  // [3>7]    V
            | ((state & 0x0000_F000) >> 8)   // [4>6]  [2.0]
            | ((state & 0x0000_00F0) << 8)   // [6>4]  [3.7]
            | ((state & 0x0000_000F) << 16); // [7>3]  [4.6]

        uint mirror2 = (state & 0x00F000F0)  //        [01.]
            | ((state & 0xF000_0000) >> 16)  // [0>4]  [7.3]
            | ((state & 0x0F00_0000) >> 8)   // [1>3]  [.54]
            | ((state & 0x000F_0000) << 8)   // [3>1]    V
            | ((state & 0x0000_F000) << 16)  // [4>0]  [43.]
            | ((state & 0x0000_0F00) >> 8)   // [5>7]  [5.1]
            | ((state & 0x0000_000F) << 8);  // [7>5]  [.70]

        uint mirror3 = (state & 0x000F000F)  //        [012]
            | ((state & 0xF000_0000) >> 24)  // [0>6]  [...]
            | ((state & 0x0F00_0000) >> 16)  // [1>5]  [654]
            | ((state & 0x00F0_0000) >> 8)   // [2>4]    V
            | ((state & 0x0000_F000) << 8)   // [4>2]  [654]
            | ((state & 0x0000_0F00) << 16)  // [5>1]  [...]
            | ((state & 0x0000_00F0) << 24); // [6>0]  [012]

        return Math.Min(NormalizeRotation(state), Math.Min(
            Math.Min(NormalizeRotation(mirror0), NormalizeRotation(mirror1)),
            Math.Min(NormalizeRotation(mirror2), NormalizeRotation(mirror3))));
    }

    public static int CheckWinner(ulong state)
    {
        int slot11 = (int)(state >> 32) & 0x3;
        int slot00 = (int)(state >> 28) & 0x3;
        int slot01 = (int)(state >> 24) & 0x3;
        int slot02 = (int)(state >> 20) & 0x3;
        int slot12 = (int)(state >> 16) & 0x3;
        int slot22 = (int)(state >> 12) & 0x3;
        int slot21 = (int)(state >> 8) & 0x3;
        int slot20 = (int)(state >> 4) & 0x3;
        int slot10 = (int)(state >> 0) & 0x3;

        if (slot00 != 0
            && ((slot00 == slot01 && slot01 == slot02)
            || (slot00 == slot10 && slot10 == slot20)))
            return slot00;
        if (slot11 != 0
            && ((slot00 == slot11 && slot11 == slot22)
            || (slot02 == slot11 && slot11 == slot20)
            || (slot01 == slot11 && slot11 == slot21)
            || (slot10 == slot11 && slot11 == slot12)))
            return slot11;
        if (slot22 != 0
            && ((slot22 == slot21 && slot21 == slot20)
            || (slot22 == slot12 && slot12 == slot02)))
            return slot22;
        return 0;
    }

    public static uint StepOf(ulong state)
    {
        return (uint)(state >> 36) + 1;
    }

    public static ulong WithStep(ulong state, uint step)
    {
        return ((ulong)step << 36) | (state & 0x0000_000F_FFFF_FFFF);
    }

    public static string ToString(ulong state, bool complex = true)
    {
        char slot11 = " XO?"[(int)(state >> 32) & 0x3];
        char slot00 = " XO?"[(int)(state >> 28) & 0x3];
        char slot01 = " XO?"[(int)(state >> 24) & 0x3];
        char slot02 = " XO?"[(int)(state >> 20) & 0x3];
        char slot12 = " XO?"[(int)(state >> 16) & 0x3];
        char slot22 = " XO?"[(int)(state >> 12) & 0x3];
        char slot21 = " XO?"[(int)(state >> 8) & 0x3];
        char slot20 = " XO?"[(int)(state >> 4) & 0x3];
        char slot10 = " XO?"[(int)(state >> 0) & 0x3];
        if (complex)
        {
            uint step = StepOf(state);
            return $"""
                Round:{step >> 1}
                Player:{(step & 1) + 1}
                [{slot00}{slot01}{slot02}]
                [{slot10}{slot11}{slot12}]
                [{slot20}{slot21}{slot22}]
                """;
        }
        return $"""
            [{slot00}{slot01}{slot02}]
            [{slot10}{slot11}{slot12}]
            [{slot20}{slot21}{slot22}]
            """;
    }
}