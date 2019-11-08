using System;
using UnityEngine;
using Firebase.Storage;
using System.Collections.Generic;

/// <summary>
/// Data container for a recipe
/// </summary>
[Serializable]
public class Recipe 
{
    
    public string Name;
    public int Calories;
    public int PrepTimeMinutes;
    public Ingredient[] Ingredients;
    public string[] Steps;
    public int StarRating;
    public Review[] Reviews;
    public string[] Tags;
    public string ImageReferencePath;
    public Sprite ImageSprite;
    public string Key;
    public string RootImagePath;
    public string UID;
    /// <summary>
    /// Creates a new instance of Recipe
    /// </summary>
    /// <param name="name">The name of the recipe</param>
    /// <param name="imagePath">Image itemPath url for the database</param>
    /// <param name="calories">The amount of calories</param>
    /// <param name="prepTimeMinutes">The prep time in minutes</param>
    /// <param name="tags">The tags for this recipe</param>
    /// <param name="ingredients">The ingredients for this recipe</param>
    /// <param name="steps">The steps for this recipe</param>
    /// <param name="reviews">The reviews for this recipe</param>
    /// <param name="starRating">The rating for this recipe</param>
    public Recipe(string name, string imagePath, int calories, int prepTimeMinutes, List<string> tags, List<Ingredient> ingredients, List<string> steps, List<Review> reviews, int starRating, string key = "")
    {
        
        Name = name;
        ImageReferencePath = imagePath;
        Calories = calories;
        PrepTimeMinutes = prepTimeMinutes;
        Tags = tags.ToArray();
        Ingredients = ingredients.ToArray();
        Steps = steps.ToArray();
        StarRating = starRating;
        this.UID = UID;
        if(reviews != null)
            Reviews = reviews.ToArray();

        Key = key;


    }


    public override string ToString()
    {
        String recipeString = "";

        recipeString += "Name: " + Name;
        recipeString += "\nCalories: " + Calories;
        recipeString += "\nPrep Time: " + PrepTimeMinutes + " minutes";
        recipeString += "\n\nIngredients: ";

        foreach (var ingredient in Ingredients)
            recipeString += "\n" + ingredient;

        recipeString += "\n\nSteps: ";

        foreach (var step in Steps)
            recipeString += "\n\n" + step;
            
        recipeString += "\n\n" + RootImagePath;

        return recipeString;
    }
}
