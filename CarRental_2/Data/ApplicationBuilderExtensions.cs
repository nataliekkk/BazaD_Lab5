namespace CarRental_2.Data
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseDbInitializer(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CarRental_2Context>();
                DbInitializer.Initialize(context); // Синхронный метод
            }
        }
    }
}
