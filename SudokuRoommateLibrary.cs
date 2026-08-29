using StardewValley;

namespace HeyYoureCursed;

internal sealed class SudokuRoommateDialogue
{
    public string[] Lines { get; init; } = Array.Empty<string>();
    public int PortraitIndex { get; init; }
}

/// <summary>
/// Natural roommate dialogue, deliberately separate from puzzle progression.
/// Stage clears represent Puzzle Bond; Trust represents ordinary time spent together.
/// </summary>
internal static class SudokuRoommateLibrary
{
    private static readonly SudokuRoommateDialogue[][] TalkPools =
    {
        new[]
        {
            D(6, "...Ngươi muốn nói chuyện?", "Ta không có gì để nói. ...Nhưng ngươi có thể ngồi đó."),
            D(1, "Người sống nói chuyện rất nhiều.", "Ngươi thì... tạm chịu được."),
            D(0, "Đừng hỏi ta chết thế nào.", "Ta cũng chưa hỏi tại sao ngươi trồng từng ấy củ cải."),
            D(6, "Ngươi không cần giải bảng mà vẫn đứng đây sao?", "...Kỳ lạ.")
        },
        new[]
        {
            D(0, "Hôm nay ngươi về sớm hơn.", "Ta chỉ nhận ra thôi. Đừng nghĩ nhiều."),
            D(2, "Ta bắt đầu phân biệt được tiếng chân của ngươi.", "Điều đó không có nghĩa là ta chờ."),
            D(5, "Ngươi có một thói quen rất lạ.", "Ngày nào cũng quay lại nói chuyện với một con ma."),
            D(0, "Căn nhà này bớt yên hơn trước.", "...Không tệ.")
        },
        new[]
        {
            D(4, "Ta đã định kể ngươi một chuyện.", "...Quên rồi. Mai hỏi lại."),
            D(5, "Hôm nay của ngươi thế nào?", "Không cần kể hết. Ta chỉ... hỏi thôi."),
            D(4, "Ta nghĩ ta đã quen với tiếng cửa mở khi ngươi về.", "Đừng thay cái cửa."),
            D(3, "Ngươi không cần lúc nào cũng mang bảng ra.", "Ngồi một chút cũng được.")
        },
        new[]
        {
            D(3, "Ngươi về rồi.", "...Tốt."),
            D(4, "Ta thích những ngày ngươi ở nhà lâu hơn.", "Đừng bắt ta nói lại."),
            D(3, "Có lẽ người sống gọi thứ này là... quen thuộc.", "Ta chưa quyết định có thích từ đó không."),
            D(4, "Nếu một ngày ta không ở cạnh TV nữa...", "Ngươi vẫn sẽ tìm ta chứ?")
        },
        new[]
        {
            D(4, "Ta không còn cần bảng Sudoku để giữ ngươi ngồi lại nữa, phải không?", "...Tốt."),
            D(3, "Ta đã đợi ngươi.", "Lần này ta không định phủ nhận."),
            D(4, "Ngôi nhà này trước đây là của ngươi.", "Bây giờ... có lẽ ta cũng ở đây."),
            D(3, "Hôm nay không chơi cũng được.", "Ta chỉ muốn biết ngươi đã về.")
        }
    };

    private static readonly Dictionary<SudokuActivityKind, SudokuRoommateDialogue[]> ActivityPools = new()
    {
        [SudokuActivityKind.TvWatch] = new[]
        {
            D(1, "Ta đang nhìn cái TV.", "Nó yên lặng hơn nơi ta từng ở bên kia."),
            D(0, "TV vừa nhiễu một nhịp khi ngươi bước vào.", "Không phải ta. ...Lần này."),
            D(4, "Ta đang xem cái màn hình đen.", "Nó phản chiếu căn nhà khá rõ. Cả ngươi nữa.")
        },
        [SudokuActivityKind.FurnitureWatch] = new[]
        {
            D(0, "Ta đang quan sát đồ đạc của ngươi.", "Người sống có rất nhiều thứ chỉ để... đặt những thứ khác lên trên."),
            D(5, "Cái ghế này ở vị trí khác hôm qua.", "Ta không di chuyển nó. Có lẽ."),
            D(4, "Ta đang học cách một căn nhà thay đổi khi có người sống trong đó.", "Ngươi thay đổi nó khá nhiều.")
        },
        [SudokuActivityKind.PetWatch] = new[]
        {
            D(2, "Nó nhìn thấy ta.", "Ta nghĩ nó biết ta đứng đây từ trước khi ngươi biết."),
            D(4, "Con vật này không sợ ta.", "...Ta chưa quyết định đó là xúc phạm hay lời khen."),
            D(3, "Nó cứ đi theo ta bằng mắt.", "Có lẽ nó thích ta hơn ngươi.")
        },
        [SudokuActivityKind.HouseListening] = new[]
        {
            D(6, "Ta đang nghe căn nhà.", "Gỗ kêu khác nhau ở từng giờ."),
            D(0, "Ta đang đếm những tiếng động trong tường.", "Đến bảy mươi ba thì ngươi thường mở cửa."),
            D(4, "Căn nhà có âm thanh khác khi ngươi ở trong này.", "Ta nhận ra điều đó gần đây.")
        },
        [SudokuActivityKind.DoorWatch] = new[]
        {
            D(5, "Ta đang nghe cửa.", "Ta biết ngươi sắp về trước khi tay ngươi chạm vào nó."),
            D(0, "Người sống luôn dùng cửa.", "Rất có nguyên tắc."),
            D(3, "Ta đứng đây một lúc thôi.", "...Không phải để đợi ngươi.")
        },
        [SudokuActivityKind.QuietCorner] = new[]
        {
            D(6, "Góc này yên hơn.", "Ta thích nghe ngôi nhà từ đây."),
            D(1, "Ta không trốn.", "Ta chỉ chọn chỗ ít người sống đi ngang qua."),
            D(0, "...Không làm gì cả.", "Người sống cũng làm vậy đôi khi, đúng không?")
        },
        [SudokuActivityKind.WaitingForPlayer] = new[]
        {
            D(4, "Ta đang đợi ngươi về.", "...Đó cũng được tính là một hoạt động."),
            D(3, "Ta đã để chỗ cạnh đây trống.", "Không có lý do gì đặc biệt."),
            D(4, "Ta định chọn một bảng cho ngươi.", "Rồi ta nhớ ra chúng ta không nhất thiết phải chơi mới nói chuyện được.")
        }
    };

    private static readonly SudokuRoommateDialogue[] StrangePool =
    {
        D(6, "TV vừa bật một khung hình mà ta không nhận ra.", "Ta đã tắt nó. Có lẽ."),
        D(1, "Có thứ gì đó đứng ngoài cửa lúc nãy.", "Nó bỏ đi khi ta nhìn lại."),
        D(0, "Gương trong nhà phản chiếu ngươi chậm hơn một nhịp.", "...Đừng lo. Hôm nay thôi."),
        D(5, "Con vật trong nhà cứ nhìn qua vai ngươi.", "Ta không đứng ở đó."),
        D(2, "Đêm qua có tiếng gõ từ phía trong TV.", "Ta không trả lời.")
    };

    public static SudokuRoommateDialogue GetNaturalTalk(int trust, int stageClears)
    {
        int level = GetTrustLevel(trust);
        SudokuRoommateDialogue[] pool = TalkPools[level];
        return Pick(pool, 0x51A7L + stageClears * 17L + trust * 101L);
    }

    public static SudokuRoommateDialogue GetActivity(SudokuActivityKind activity, int trust)
    {
        SudokuRoommateDialogue[] pool = ActivityPools.TryGetValue(activity, out SudokuRoommateDialogue[]? found)
            ? found
            : ActivityPools[SudokuActivityKind.HouseListening];

        return Pick(pool, 0xA671L + trust * 37L + (int)activity * 997L + Game1.timeOfDay);
    }

    public static SudokuRoommateDialogue GetStrangeThing(int trust)
    {
        return Pick(StrangePool, 0x6A057L + trust * 53L);
    }

    public static string GetWelcomeHomeLine(int trust)
    {
        string[] lines = trust switch
        {
            <= 6 => new[]
            {
                "Sudoku khẽ quay đầu. \"...Ngươi về rồi.\"",
                "Sudoku nhìn về phía cửa. \"Ta nghe thấy ngươi từ ngoài kia.\""
            },
            <= 12 => new[]
            {
                "Sudoku nhìn bạn một lúc. \"Hôm nay ngươi về muộn hơn.\"",
                "\"...Ngươi về rồi.\" Sudoku quay lại việc đang làm."
            },
            <= 20 => new[]
            {
                "Sudoku khẽ nghiêng đầu. \"Ngươi về rồi. ...Tốt.\"",
                "\"Ta biết là ngươi sẽ về.\" Sudoku nói như thể đó là chuyện hiển nhiên."
            },
            _ => new[]
            {
                "\"Ngươi về rồi.\" Sudoku có vẻ nhẹ nhõm hơn cô ấy muốn thừa nhận.",
                "Sudoku nhìn sang bạn. \"Ta đã đợi ngươi.\""
            }
        };

        long raw = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)Game1.Date.TotalDays * 65537L) ^ trust * 313L);
        int index = (int)((raw & long.MaxValue) % lines.Length);
        return lines[index];
    }

    public static string GetActivityLabel(SudokuActivityKind activity)
    {
        return activity switch
        {
            SudokuActivityKind.TvWatch => "Đang nhìn TV",
            SudokuActivityKind.FurnitureWatch => "Đang quan sát đồ đạc",
            SudokuActivityKind.PetWatch => "Đang quan sát thú nuôi",
            SudokuActivityKind.HouseListening => "Đang nghe căn nhà",
            SudokuActivityKind.DoorWatch => "Đang đứng gần cửa",
            SudokuActivityKind.QuietCorner => "Đang ở một góc yên tĩnh",
            SudokuActivityKind.WaitingForPlayer => "Đang đợi bạn về",
            _ => "Đang ở trong nhà"
        };
    }

    public static string GetTrustLabel(int trust)
    {
        return trust switch
        {
            <= 2 => "Xa lạ",
            <= 6 => "Bớt đề phòng",
            <= 12 => "Quen thuộc",
            <= 20 => "Tin tưởng",
            _ => "Như người nhà"
        };
    }

    private static int GetTrustLevel(int trust)
    {
        return trust switch
        {
            <= 2 => 0,
            <= 6 => 1,
            <= 12 => 2,
            <= 20 => 3,
            _ => 4
        };
    }

    private static SudokuRoommateDialogue Pick(IReadOnlyList<SudokuRoommateDialogue> pool, long salt)
    {
        long raw = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)Game1.Date.TotalDays * 104729L) ^ salt);
        int index = (int)((raw & long.MaxValue) % pool.Count);
        return pool[index];
    }

    private static SudokuRoommateDialogue D(int portrait, params string[] lines)
    {
        return new SudokuRoommateDialogue
        {
            PortraitIndex = portrait,
            Lines = lines
        };
    }
}
