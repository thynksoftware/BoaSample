namespace Boa.Sample.Services
{
    public interface IViewRenderService
    {
        string Render(string viewPath);

        string Render<TModel>(string viewPath, TModel model);
    }
}