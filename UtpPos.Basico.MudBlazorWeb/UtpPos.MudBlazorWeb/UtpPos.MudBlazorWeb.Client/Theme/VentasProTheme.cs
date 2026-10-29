using MudBlazor;

namespace UtpPos.MudBlazorWeb.Client.Theme;

public static class VentasProTheme
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3EBBD0",
            Secondary = "#3D67B6",
            Background = "#F3F6FC",
            Surface = "#FFFFFF",
            AppbarBackground = "#3EBBD0",
            AppbarText = "#12303A",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",
            DrawerIcon = "#64748B",
            TextPrimary = "#172033",
            TextSecondary = "#64748B",
            Divider = "#DDE5F0",
            LinesDefault = "#DDE5F0",
            Success = "#159F68",
            Warning = "#E59A25",
            Error = "#DC4C64",
            Info = "#3D67B6"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#58C8D9",
            Secondary = "#7D9FE0",
            Background = "#101827",
            Surface = "#182235",
            AppbarBackground = "#3EBBD0",
            AppbarText = "#102C34",
            DrawerBackground = "#151F31",
            DrawerText = "#D8E2F0",
            DrawerIcon = "#AFC0D5",
            TextPrimary = "#F5F8FC",
            TextSecondary = "#AFC0D5",
            Divider = "#2A3A50",
            Success = "#38C98B",
            Warning = "#F2B45B",
            Error = "#F07186"
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Poppins", "Segoe UI", "sans-serif"]
            }
        }
    };
}
