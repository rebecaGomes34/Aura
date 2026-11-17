
using Aura.Views;

namespace Aura;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // STACK: rotas para páginas de detalhes
        Routing.RegisterRoute(
            nameof(ArtworkDetailsPage),
            typeof(ArtworkDetailsPage));
    }
}
