using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the recipe view button to be instantiated and initialized in the main screen
/// </summary>
public class RecipeButtonView : MonoBehaviour
{
    [SerializeField] private Image recipeImage;
    [SerializeField] private Text recipeName;
    [SerializeField] private GameObject loadingPanelObject;


    private Recipe recipe;
    private static bool isCurrentRequestDone = true;

    private void Awake()
    {
        //Test();
    }

    /// <summary>
    /// Initialize the recipe button
    /// </summary>
    /// <param name="newRecipe">Recipe whose info will be used to initialize the button</param>
    public void InitRecipeButton(Recipe newRecipe)
    {
        print("RecipeButtonView>InitRecipeBtutton: " + newRecipe);
        print("RecipeButtonView>InitRecipeBtutton: Null?: ");
        print( newRecipe == null);

        if (newRecipe == null)
            return;

        // turn on loading panel until the image is retrieved
        loadingPanelObject.SetActive(true);
        recipeName.text = newRecipe.Name;
        recipe = newRecipe;

        recipeImage.sprite = null;
        GetSprite();

        GetComponentInChildren<Button>().onClick.AddListener(OpenRecipe);
    }

    /// <summary>
    /// Reference the recipe manager to open the recipe UI and initialize the components with this recipe
    /// </summary>
    public void OpenRecipe()
    {
        RecipeManagerUI.Instance.InitRecipeUI(recipe);
    }

 
    private void GetSprite()
    {
        GameObject imageRequest = new GameObject("Image Request");
        ImageRequest newRequest = imageRequest.AddComponent<ImageRequest>();
        newRequest.Init(recipe.ImageReferencePath, recipeImage);

        StartCoroutine(WaitForImage());
    }

    private IEnumerator WaitToRequestImage()
    {
        yield return new WaitUntil(() => isCurrentRequestDone);

    }

    private IEnumerator WaitForImage()
    {
        yield return new WaitWhile(() => recipeImage.sprite == null);

        loadingPanelObject.SetActive(false);
        recipe.ImageSprite = recipeImage.sprite;
    }

    private void Test()
    {
        List<Ingredient> ingredients = new List<Ingredient>()
        {
            new Ingredient("flour", "1/2 cup"),
            new Ingredient("marinara", "1/2 cup"),
            new Ingredient("mozzerella", "2 cups"),
            new Ingredient("ham", "1/3 cup"),
            new Ingredient("pineapple", "1/4 cup")
        };

        List<string> steps = new List<string>()
        {
            "Knead the dough.",
            "Add the marinara sauce.",
            "Add the mozerrella cheese.",
            "Add the ham.",
            "Add the pineapple.",
            "Bake at 360F for 45 minutes."
        };

        List<string> tags = new List<string>()
        {
            "dairy"
        };

        List<Review> reviews = new List<Review>()
        {
            
        };

        Recipe newRecipe = new Recipe("Garlic Salmon", "gs://regen-66cf8.appspot.com/Recipes/garlicsalmon.jpg", 450, 50, tags, ingredients, steps,  reviews, 4);

        InitRecipeButton(newRecipe);
    }


}
