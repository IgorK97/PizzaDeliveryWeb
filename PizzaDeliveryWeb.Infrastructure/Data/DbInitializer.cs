using Microsoft.AspNetCore.Identity;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(PizzaDeliveringContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Проверяем наличие ролей
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
                await roleManager.CreateAsync(new IdentityRole("client"));
                await roleManager.CreateAsync(new IdentityRole("courier"));
            }

            // Проверяем наличие пользователей
            if (!userManager.Users.Any())
            {
                var adminUser = new User { FirstName="Имя", LastName="Фамилия", Surname = "Отчество", UserName = "admin", Email = "admin@example.com" };
                var userCreationResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (userCreationResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "admin");
                }

                var normalUser = new User { FirstName = "Имя", LastName = "Фамилия", Surname = "Отчество", UserName = "user", Email = "user@example.com" };
                var normalUserCreationResult = await userManager.CreateAsync(normalUser, "User@123");
                if (normalUserCreationResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "client");
                }
            }

            // Добавляем тестовые данные пицц и ингредиентов, размеров пицц, статусов заказов
            if (!context.Pizzas.Any())
            {
                var pizzas = new[]
            {
                new Pizza { Name = "Ветчина и сыр", IsAvailable = true, Description = "Вкусная пицца с ветчиной, моцареллой и соусом альфредо", Image="/images/pizzas/hamandcheese.png" },
                new Pizza { Name = "Сырная", IsAvailable = true, Description = "Традиционная сырная пицца с моцареллой, сырами чеддер и пармезан и соусом альфредо", Image="/images/pizzas/cheesePizza.png" },
                new Pizza { Name = "Мясная", IsAvailable = true, Description = "Нежный цыпленок, ветчина, пепперони, острая чоризо, моцарелла, фирменный томатный соус...", Image="/images/pizzas/meat.png" },
                new Pizza { Name = "Мясное барбекю", IsAvailable = true, Description = "Пепперони, альпийские колбаски, моцарелла и соус барбекю - отличный вариант для пиццы!" , Image = "/images/pizzas/bbq.png"},
                new Pizza { Name = "Маргарита", IsAvailable = true, Description = "Известная пицца с моцареллой, томатами, итальянскими травами и фирменным томатным соусом" , Image = "/images/pizzas/margarita.png"},
                new Pizza { Name = "Гавайская", IsAvailable = true, Description = "Нежный цыпленок, сочный ананас, моцарелла и фирменный соус альфредо - все для любителей гавайской пиццы" , 
                    Image = "/images/pizzas/hawaii.png"},
                new Pizza { Name = "Пепперони", IsAvailable = true, Description = "Пикантная пепперони, моцарелла, свежие томаты, фирменный томатный соус" , Image = "/images/pizzas/pepperoni.png"},
                new Pizza { Name = "Лососевая", IsAvailable = true, 
                    Description = "Моцарелла, лосось, креветки, томаты, красный лук, брокколи, базилик" , Image = "/images/pizzas/salmon.png"},
                new Pizza { Name = "Вегетарианская", IsAvailable = true, Description = "Шампиньоны, томаты, перец, красный лук, брынза, моцарелла, томатный соус, итальянские травы" , 
                    Image = "/images/pizzas/vegetarian.png"},
                new Pizza { Name = "Пользовательская", IsAvailable = true, Description = "Не нашли пиццу, которую хотели? Тогда создайте свою!" , Image = "/images/pizzas/user.png"}
            };
                context.Pizzas.AddRange(pizzas);
                context.SaveChanges();
            }
            if (!context.Ingredients.Any())
            {
                var ingredients = new[]
            {
                new Ingredient { Name = "ветчина", Description="Вкусная ветчина", PricePerGram = 2.00m,Small=15.00m, Medium = 20.00m, Big = 25.00m, IsAvailable = true, Image="/images/ingredients/ham.png" },
                new Ingredient { Name = "моцарелла", Description="Вкусная моцарелла",PricePerGram = 2.00m,Small=15.00m, Medium = 25.00m, Big = 35.00m, IsAvailable = true, Image="/images/ingredients/mozarella.png" },
                new Ingredient { Name = "соус альфредо", Description="Вкусный соус альфредо",PricePerGram = 2.00m, Small=15.00m,Medium = 25.00m, Big = 35.00m, IsAvailable = true, Image="/images/ingredients/alfredo" },
                new Ingredient { Name = "сыры чеддер и пармезан", Description="Вкусные сыры чеддер и пармезан",PricePerGram = 3.00m, Small=15.00m,Medium = 20.00m, Big = 30.00m, IsAvailable = true, Image="/images/ingredients/cheese.png" },
                new Ingredient { Name = "цыпленок", Description="Вкусный цыпленок",PricePerGram = 3.00m,Small=15.00m, Medium = 30.00m, Big = 40.00m, IsAvailable = true, Image="/images/ingredients/chicken.png" },

            };

                context.Ingredients.AddRange(ingredients);
                context.SaveChanges();
            }
            //var pizza1 = context.Pizzas.FirstOrDefault(p => p.Name == "Ветчина и сыр");
            //var pizza2 = context.Pizzas.FirstOrDefault(p => p.Name == "Сырная");

            //var ingredient1 = context.Ingredients.FirstOrDefault(i => i.Name == "ветчина");
            //var ingredient2 = context.Ingredients.FirstOrDefault(i => i.Name == "моцарелла");

            //if (pizza1 != null && ingredient1 != null)
            //{
            //    pizza1.Ingredients.Add(ingredient1);
            //}
            //if (pizza2 != null && ingredient2 != null)
            //{
            //    pizza2.Ingredients.Add(ingredient2);
            //}

            if (!context.DelStatuses.Any())
            {
                var orderStatuses = new[]
                {
                new DelStatus { Description = "Не оформлен" },
                new DelStatus { Description = "Формируется" },
                new DelStatus { Description = "Готовится" },
                new DelStatus { Description = "Передается в доставку" },
                new DelStatus { Description = "Передан курьеру" },
                new DelStatus { Description = "Отменен" },
                new DelStatus { Description = "Доставлен" },
                new DelStatus { Description = "Не доставлен" }

            };

                context.DelStatuses.AddRange(orderStatuses);
                context.SaveChanges();
            }

            if (!context.PizzaSizes.Any())
            {
                var pizzaSizes = new[]
                {
                new PizzaSize { Name = "Маленькая", Price=200m, Weight=250m },
                new PizzaSize { Name = "Средняя", Price=300m, Weight=370m },
                new PizzaSize { Name = "Большая", Price=400m, Weight=510m },

            };

                context.PizzaSizes.AddRange(pizzaSizes);
                context.SaveChanges();
            }

            await context.SaveChangesAsync();
         
        }

    }
}
