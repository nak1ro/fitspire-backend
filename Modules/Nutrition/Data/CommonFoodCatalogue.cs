using backend.Modules.Nutrition.Domain.Enums;

namespace backend.Modules.Nutrition.Data;

public static class CommonFoodCatalogue
{
    public static CommonFoodSeed[] Definitions =>
    [
        // Fruits
        new("banana", "Banana", "Fruits", 1, 1, QuantityUnit.Pieces, null, 105, 1.3m, 27, 0.4m),
        new("apple", "Apple", "Fruits", 2, 1, QuantityUnit.Pieces, null, 95, 0.5m, 25, 0.3m),
        new("orange", "Orange", "Fruits", 3, 1, QuantityUnit.Pieces, null, 62, 1.2m, 15, 0.2m),
        new("strawberries", "Strawberries", "Fruits", 4, 1, QuantityUnit.CustomServing, "cup", 49, 1, 12, 0.5m),
        new("blueberries", "Blueberries", "Fruits", 5, 1, QuantityUnit.CustomServing, "cup", 84, 1.1m, 21, 0.5m),
        new("grapes", "Grapes", "Fruits", 6, 1, QuantityUnit.CustomServing, "cup", 104, 1.1m, 27, 0.2m),
        new("avocado", "Avocado", "Fruits", 7, 0.5m, QuantityUnit.Pieces, null, 160, 2, 8.5m, 15),
        new("watermelon", "Watermelon", "Fruits", 8, 1, QuantityUnit.CustomServing, "cup", 46, 0.9m, 11.5m, 0.2m),
        new("mango", "Mango", "Fruits", 9, 1, QuantityUnit.CustomServing, "cup", 99, 1.4m, 25, 0.6m),
        new("pineapple", "Pineapple", "Fruits", 10, 1, QuantityUnit.CustomServing, "cup", 82, 0.9m, 22, 0.2m),

        // Vegetables
        new("broccoli", "Broccoli", "Vegetables", 1, 1, QuantityUnit.CustomServing, "cup", 31, 2.6m, 6, 0.3m),
        new("spinach", "Spinach (raw)", "Vegetables", 2, 1, QuantityUnit.CustomServing, "cup", 7, 0.9m, 1.1m, 0.1m),
        new("carrot", "Carrot", "Vegetables", 3, 1, QuantityUnit.Pieces, null, 25, 0.6m, 6, 0.1m),
        new("sweet-potato", "Sweet potato", "Vegetables", 4, 1, QuantityUnit.Pieces, null, 112, 2, 26, 0.1m),
        new("potato", "Potato", "Vegetables", 5, 1, QuantityUnit.Pieces, null, 161, 4.3m, 37, 0.2m),
        new("tomato", "Tomato", "Vegetables", 6, 1, QuantityUnit.Pieces, null, 22, 1.1m, 4.8m, 0.2m),
        new("cucumber", "Cucumber", "Vegetables", 7, 1, QuantityUnit.CustomServing, "cup", 16, 0.7m, 3.8m, 0.1m),
        new("bell-pepper", "Bell pepper", "Vegetables", 8, 1, QuantityUnit.CustomServing, "cup", 46, 1.5m, 9, 0.4m),
        new("green-beans", "Green beans", "Vegetables", 9, 1, QuantityUnit.CustomServing, "cup", 31, 1.8m, 7, 0.2m),
        new("onion", "Onion", "Vegetables", 10, 1, QuantityUnit.Pieces, null, 44, 1.2m, 10, 0.1m),

        // Protein
        new("chicken-breast", "Chicken breast, cooked", "Protein", 1, 100, QuantityUnit.Grams, null, 165, 31, 0, 3.6m),
        new("egg", "Egg, large", "Protein", 2, 1, QuantityUnit.Pieces, null, 72, 6.3m, 0.4m, 4.8m),
        new("salmon", "Salmon, cooked", "Protein", 3, 100, QuantityUnit.Grams, null, 208, 20, 0, 13),
        new("ground-beef", "Ground beef (90/10), cooked", "Protein", 4, 100, QuantityUnit.Grams, null, 176, 20, 0, 10),
        new("turkey-breast", "Turkey breast, cooked", "Protein", 5, 100, QuantityUnit.Grams, null, 135, 30, 0, 1),
        new("tofu", "Tofu, firm", "Protein", 6, 100, QuantityUnit.Grams, null, 144, 15, 3, 8.7m),
        new("shrimp", "Shrimp, cooked", "Protein", 7, 100, QuantityUnit.Grams, null, 99, 24, 0.2m, 0.3m),
        new("tuna", "Tuna, canned in water", "Protein", 8, 100, QuantityUnit.Grams, null, 116, 26, 0, 0.8m),
        new("pork-chop", "Pork chop, cooked", "Protein", 9, 100, QuantityUnit.Grams, null, 231, 25, 0, 14),
        new("black-beans", "Black beans, cooked", "Protein", 10, 1, QuantityUnit.CustomServing, "cup", 227, 15, 41, 0.9m),
        new("chickpeas", "Chickpeas, cooked", "Protein", 11, 1, QuantityUnit.CustomServing, "cup", 269, 14.5m, 45, 4.2m),
        new("lentils", "Lentils, cooked", "Protein", 12, 1, QuantityUnit.CustomServing, "cup", 230, 18, 40, 0.8m),

        // Dairy & Eggs
        new("milk-whole", "Milk, whole", "Dairy & Eggs", 1, 244, QuantityUnit.Millilitres, null, 149, 8, 12, 8),
        new("milk-skim", "Milk, skim", "Dairy & Eggs", 2, 245, QuantityUnit.Millilitres, null, 83, 8.3m, 12, 0.2m),
        new("greek-yogurt", "Greek yogurt, plain nonfat", "Dairy & Eggs", 3, 1, QuantityUnit.CustomServing, "cup", 133, 23, 9, 0.4m),
        new("cheddar", "Cheddar cheese", "Dairy & Eggs", 4, 1, QuantityUnit.CustomServing, "slice", 113, 7, 0.4m, 9.3m),
        new("cottage-cheese", "Cottage cheese, low-fat", "Dairy & Eggs", 5, 1, QuantityUnit.CustomServing, "cup", 163, 28, 6, 2.3m),
        new("butter", "Butter", "Dairy & Eggs", 6, 1, QuantityUnit.CustomServing, "tbsp", 102, 0.1m, 0, 11.5m),

        // Grains & Carbs
        new("white-rice", "White rice, cooked", "Grains & Carbs", 1, 1, QuantityUnit.CustomServing, "cup", 205, 4.3m, 45, 0.4m),
        new("brown-rice", "Brown rice, cooked", "Grains & Carbs", 2, 1, QuantityUnit.CustomServing, "cup", 216, 5, 45, 1.8m),
        new("oats", "Oats, dry", "Grains & Carbs", 3, 0.5m, QuantityUnit.CustomServing, "cup", 150, 5, 27, 2.5m),
        new("whole-wheat-bread", "Whole wheat bread", "Grains & Carbs", 4, 1, QuantityUnit.CustomServing, "slice", 69, 3.6m, 12, 0.9m),
        new("white-bread", "White bread", "Grains & Carbs", 5, 1, QuantityUnit.CustomServing, "slice", 67, 2, 12.7m, 0.8m),
        new("pasta", "Pasta, cooked", "Grains & Carbs", 6, 1, QuantityUnit.CustomServing, "cup", 221, 8, 43, 1.3m),
        new("quinoa", "Quinoa, cooked", "Grains & Carbs", 7, 1, QuantityUnit.CustomServing, "cup", 222, 8, 39, 3.6m),
        new("bagel", "Bagel", "Grains & Carbs", 8, 1, QuantityUnit.Pieces, null, 277, 11, 55, 1.7m),

        // Fats & Nuts
        new("almonds", "Almonds", "Fats & Nuts", 1, 28, QuantityUnit.Grams, null, 164, 6, 6, 14),
        new("peanut-butter", "Peanut butter", "Fats & Nuts", 2, 2, QuantityUnit.CustomServing, "tbsp", 188, 8, 6, 16),
        new("walnuts", "Walnuts", "Fats & Nuts", 3, 28, QuantityUnit.Grams, null, 185, 4.3m, 3.9m, 18.5m),
        new("olive-oil", "Olive oil", "Fats & Nuts", 4, 1, QuantityUnit.CustomServing, "tbsp", 119, 0, 0, 13.5m),
        new("chia-seeds", "Chia seeds", "Fats & Nuts", 5, 1, QuantityUnit.CustomServing, "tbsp", 58, 2, 5, 3.7m),

        // Beverages & Other
        new("coffee-black", "Coffee, black", "Beverages & Other", 1, 240, QuantityUnit.Millilitres, null, 2, 0.3m, 0, 0),
        new("orange-juice", "Orange juice", "Beverages & Other", 2, 248, QuantityUnit.Millilitres, null, 112, 1.7m, 26, 0.5m),
        new("protein-shake", "Protein shake (whey)", "Beverages & Other", 3, 1, QuantityUnit.CustomServing, "scoop", 120, 24, 3, 1.5m),
        new("honey", "Honey", "Beverages & Other", 4, 1, QuantityUnit.CustomServing, "tbsp", 64, 0.1m, 17, 0),
        new("dark-chocolate", "Dark chocolate (70%)", "Beverages & Other", 5, 28, QuantityUnit.Grams, null, 170, 2.2m, 13, 12),
    ];
}

public sealed record CommonFoodSeed(
    string Code,
    string Name,
    string Category,
    int DisplayOrder,
    decimal Quantity,
    QuantityUnit QuantityUnit,
    string? CustomUnitName,
    decimal? CaloriesKcal,
    decimal? ProteinGrams,
    decimal? CarbsGrams,
    decimal? FatGrams);
