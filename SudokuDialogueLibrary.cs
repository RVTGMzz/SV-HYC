using StardewValley;

namespace HeyYoureCursed;

internal sealed class SudokuDailyDialogue
{
    public string[] Lines { get; init; } = Array.Empty<string>();
    public string Question { get; init; } = string.Empty;
    public int PortraitIndex { get; init; }
}

internal static class SudokuDialogueLibrary
{
    private static readonly SudokuDailyDialogue[][] FreshPools =
    {
        new[]
        {
            D(6, new[] { "...Ngươi lại đến.", "Đừng nhìn ta lâu như thế. Nhìn bảng đi." }, "...Hôm nay ở lại với ta chứ?"),
            D(1, new[] { "Ta nghe thấy tiếng chân của ngươi từ bên kia màn hình.", "Đừng làm sai quá nhiều." }, "Ngươi định làm gì hôm nay?"),
            D(0, new[] { "Hôm nay ta không định bò ra khỏi TV.", "...Nếu ngươi giải tử tế." }, "...Ở lại một lát chứ?"),
            D(6, new[] { "Ngươi vẫn còn ở đây.", "Ta đã nghĩ căn nhà này sẽ đuổi ngươi đi trước ta." }, "Ngồi lại một lát nhé?"),
            D(1, new[] { "Đừng chạm vào tóc ta.", "Cũng đừng chạm vào những ô đã có số." }, "...Hiểu rồi thì ở lại." )
        },
        new[]
        {
            D(0, new[] { "Ngươi lại đến đúng giờ.", "Ít nhất ta không phải gọi qua TV." }, "Hôm nay ở lại với ta một lát chứ?"),
            D(2, new[] { "Tốc độ của ngươi... chấp nhận được.", "Đừng tự mãn." }, "Ngươi muốn làm gì tiếp?"),
            D(5, new[] { "Ta đã chọn bảng hôm nay rồi.", "Không khó đến mức phải bỏ chạy đâu." }, "Hôm nay muốn làm gì?"),
            D(0, new[] { "Ngươi bắt đầu ít làm ta đau đầu hơn.", "Chỉ một chút thôi." }, "Hôm nay ngươi muốn làm gì?"),
            D(2, new[] { "Hôm qua ngươi làm khá hơn ta nghĩ.", "...Ta không khen đâu." }, "Ở lại một lát chứ?" )
        },
        new[]
        {
            D(4, new[] { "Ta đã để dành bảng này cho ngươi.", "Đừng khiến ta hối hận." }, "Muốn ở lại với ta không?"),
            D(5, new[] { "Ngươi đến trễ hơn hôm qua.", "Ta... chỉ nhận ra thôi." }, "Ngồi lại một lát nhé?"),
            D(4, new[] { "Ta biết ngươi sẽ quay lại.", "Bảng này hợp với ngươi hơn mấy cái trước." }, "Ngươi muốn làm gì cùng ta?"),
            D(3, new[] { "Ngươi giải nhanh hơn rồi.", "Có lẽ ta nên tìm bảng khó hơn." }, "Vẫn muốn ở lại không?"),
            D(5, new[] { "Hôm nay căn nhà yên quá.", "May mà ngươi về rồi." }, "Ngồi với ta một lát nhé?" )
        },
        new[]
        {
            D(3, new[] { "Ngươi tới rồi.", "...Tốt." }, "Hôm nay ở lại với ta chứ?"),
            D(4, new[] { "Ta đã đợi ngươi.", "Không phải vì nhớ. Chỉ là căn nhà yên quá." }, "Ở lại với ta nhé?"),
            D(3, new[] { "Ta bắt đầu hiểu vì sao người sống thích thói quen.", "Ngươi về, ta đưa bảng. Khá ổn." }, "Ngồi với ta một lát nhé?"),
            D(4, new[] { "Ta giữ chỗ này cho ngươi rồi.", "Đừng bắt ta thừa nhận là ta thích có người chơi cùng." }, "Ngồi lại một lát nhé?"),
            D(3, new[] { "Hôm nay ta chọn một bảng vừa đủ khó.", "Ta muốn xem ngươi còn tiến bộ đến đâu." }, "Hôm nay ngươi muốn làm gì với ta?" )
        },
        new[]
        {
            D(4, new[] { "Mười tám bảng.", "Ngươi đã giải hết những gì ta mang theo... vậy mà vẫn quay lại." }, "Hôm nay vẫn ở lại với ta chứ?"),
            D(3, new[] { "Ta không còn Stage nào để khóa ngươi lại nữa.", "...Nhưng ta vẫn có thể tìm thêm bảng." }, "Ngồi lại với ta nhé?"),
            D(4, new[] { "Có lẽ ta đã hết lý do để bắt ngươi ở lại.", "May mà ngươi hình như không cần lý do." }, "Ở lại với ta một lát chứ?")
        }
    };

    private static readonly SudokuDailyDialogue[] RepeatPool =
    {
        D(6, new[] { "...Lại nữa?" }, "Còn muốn ở lại không?"),
        D(0, new[] { "Ngươi quay lại rồi." }, "Còn muốn làm gì nữa không?"),
        D(4, new[] { "Ta vẫn giữ bảng đây." }, "Ở lại tiếp nhé?"),
        D(3, new[] { "Ta biết ngươi sẽ quay lại." }, "Ở lại thêm một chút nhé?"),
        D(4, new[] { "...Ta vẫn ở đây." }, "Còn muốn ở đây không?")
    };

    public static SudokuDailyDialogue GetForToday(int solvedCount, bool repeatTalk, bool solvedToday)
    {
        int stage = solvedCount switch
        {
            <= 2 => 0,
            <= 6 => 1,
            <= 12 => 2,
            <= 17 => 3,
            _ => 4
        };

        SudokuDailyDialogue[] pool = repeatTalk
            ? new[] { RepeatPool[stage] }
            : FreshPools[stage];

        long raw = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)Game1.Date.TotalDays * 104729L) ^ ((long)solvedCount * 7919L));
        int index = (int)((raw & long.MaxValue) % pool.Length);
        SudokuDailyDialogue result = pool[index];

        if (!solvedToday)
            return result;

        return new SudokuDailyDialogue
        {
            Lines = result.Lines,
            PortraitIndex = result.PortraitIndex,
            Question = stage >= 2 ? "Hôm nay vẫn muốn ở lại với ta thêm không?" : "Còn muốn ở lại một chút không?"
        };
    }

    private static SudokuDailyDialogue D(int portrait, string[] lines, string question)
    {
        return new SudokuDailyDialogue
        {
            PortraitIndex = portrait,
            Lines = lines,
            Question = question
        };
    }
}
