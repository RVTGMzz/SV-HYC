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
            D(6, new[] { "...Ngươi lại đến.", "Đừng nhìn ta lâu như thế. Nhìn bảng đi." }, "...Sẵn sàng chưa?"),
            D(1, new[] { "Ta nghe thấy tiếng chân của ngươi từ bên kia màn hình.", "Đừng làm sai quá nhiều." }, "Ngươi có muốn thử không?"),
            D(0, new[] { "Hôm nay ta không định bò ra khỏi TV.", "...Nếu ngươi giải tử tế." }, "Chơi chứ?"),
            D(6, new[] { "Ngươi vẫn còn ở đây.", "Ta đã nghĩ căn nhà này sẽ đuổi ngươi đi trước ta." }, "Bắt đầu nhé?"),
            D(1, new[] { "Đừng chạm vào tóc ta.", "Cũng đừng chạm vào những ô đã có số." }, "...Hiểu rồi thì chơi." )
        },
        new[]
        {
            D(0, new[] { "Ngươi lại đến đúng giờ.", "Ít nhất ta không phải gọi qua TV." }, "Làm một bảng chứ?"),
            D(2, new[] { "Tốc độ của ngươi... chấp nhận được.", "Đừng tự mãn." }, "Chơi tiếp không?"),
            D(5, new[] { "Ta đã chọn bảng hôm nay rồi.", "Không khó đến mức phải bỏ chạy đâu." }, "Sẵn sàng chưa?"),
            D(0, new[] { "Ngươi bắt đầu ít làm ta đau đầu hơn.", "Chỉ một chút thôi." }, "Muốn thử bảng hôm nay không?"),
            D(2, new[] { "Hôm qua ngươi làm khá hơn ta nghĩ.", "...Ta không khen đâu." }, "Bắt đầu chứ?" )
        },
        new[]
        {
            D(4, new[] { "Ta đã để dành bảng này cho ngươi.", "Đừng khiến ta hối hận." }, "Muốn chơi cùng ta không?"),
            D(5, new[] { "Ngươi đến trễ hơn hôm qua.", "Ta... chỉ nhận ra thôi." }, "Bắt đầu nhé?"),
            D(4, new[] { "Ta biết ngươi sẽ quay lại.", "Bảng này hợp với ngươi hơn mấy cái trước." }, "Chơi một ván chứ?"),
            D(3, new[] { "Ngươi giải nhanh hơn rồi.", "Có lẽ ta nên tìm bảng khó hơn." }, "Vẫn muốn chơi không?"),
            D(5, new[] { "Hôm nay căn nhà yên quá.", "May mà ngươi về rồi." }, "Ngồi chơi với ta một lát nhé?" )
        },
        new[]
        {
            D(3, new[] { "Ngươi tới rồi.", "...Tốt." }, "Hôm nay chơi cùng ta chứ?"),
            D(4, new[] { "Ta đã đợi ngươi.", "Không phải vì nhớ. Chỉ là căn nhà yên quá." }, "Chơi với ta nhé?"),
            D(3, new[] { "Ta bắt đầu hiểu vì sao người sống thích thói quen.", "Ngươi về, ta đưa bảng. Khá ổn." }, "Ngồi với ta một lát nhé?"),
            D(4, new[] { "Ta giữ chỗ này cho ngươi rồi.", "Đừng bắt ta thừa nhận là ta thích có người chơi cùng." }, "Bắt đầu nhé?"),
            D(3, new[] { "Hôm nay ta chọn một bảng vừa đủ khó.", "Ta muốn xem ngươi còn tiến bộ đến đâu." }, "Chơi với ta chứ?" )
        },
        new[]
        {
            D(4, new[] { "Mười tám bảng.", "Ngươi đã giải hết những gì ta mang theo... vậy mà vẫn quay lại." }, "Hôm nay vẫn chơi với ta chứ?"),
            D(3, new[] { "Ta không còn Stage nào để khóa ngươi lại nữa.", "...Nhưng Daily Challenge thì vẫn còn." }, "Ngồi lại với ta nhé?"),
            D(4, new[] { "Có lẽ ta đã hết lý do để bắt ngươi ở lại.", "May mà ngươi hình như không cần lý do." }, "Chơi cùng ta một lát chứ?")
        }
    };

    private static readonly SudokuDailyDialogue[] RepeatPool =
    {
        D(6, new[] { "...Lại nữa?" }, "Muốn làm tiếp không?"),
        D(0, new[] { "Ngươi quay lại rồi." }, "Mở bảng tiếp chứ?"),
        D(4, new[] { "Ta vẫn giữ bảng đây." }, "Chơi tiếp nhé?"),
        D(3, new[] { "Ta biết ngươi sẽ quay lại." }, "Cùng chơi tiếp nhé?"),
        D(4, new[] { "...Ta vẫn ở đây." }, "Chơi thêm một bảng chứ?")
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
            Question = stage >= 2 ? "Hôm nay vẫn muốn chơi với ta thêm không?" : "Muốn chơi thêm một bảng không?"
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
