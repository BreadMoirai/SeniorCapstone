using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;


public class RecipeManagerUI : MonoBehaviour
{
    public static Recipe currentRecipe;
    public static RecipeManagerUI Instance;
    [SerializeField] private GameObject canvas;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private GameObject infoPrefab;
    [SerializeField] private GameObject reviewPrefab;
    [SerializeField] private GameObject moreReviewsButton;

    [Header("Dish Info")]
    [SerializeField] private Image dishImage;
    [SerializeField] private Text dishNameText;
    [SerializeField] private Text ingredientCountText;
    [SerializeField] private Text calorieCountText;
    [SerializeField] private Text prepTimeText;

    [SerializeField] private Transform starRatingTrans;
    [SerializeField] private Transform verticalGroupTrans;

    [SerializeField] private GameObject loadingObject;

    [SerializeField] private GameObject favoriteButton;
    [SerializeField] private GameObject unfavoriteButton;

    



    private Sprite currentRecipeSprite;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetSprite(Sprite newSprite)
    {
        currentRecipeSprite = newSprite;
    }

    public void InitRecipeUI(Recipe newRecipe)
    {
        currentRecipe = newRecipe;
        dishImage.sprite = newRecipe.ImageSprite;

        #region Update recipe header info

        // update text elements
        dishNameText.text = newRecipe.Name;
        ingredientCountText.text = newRecipe.Ingredients.Length.ToString("N0");
        calorieCountText.text = newRecipe.Calories.ToString("N0");
        prepTimeText.text = newRecipe.PrepTimeMinutes.ToString("N0");

        #endregion

        // remove any previous ingredients and directions
        if (verticalGroupTrans.childCount > 1)
        {
            for (int i = 1; i < verticalGroupTrans.childCount; i++)
                Destroy(verticalGroupTrans.GetChild(i).gameObject);
        }

        #region Load ingredients

        // update ingredients
        for (int i = 0; i < newRecipe.Ingredients.Length; i++)
        {
            Text infoText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                verticalGroupTrans).GetComponentInChildren<Text>();

            infoText.text = newRecipe.Ingredients[i].ToString();
        }

        #endregion

        #region Load directions

        // create directions label
        Text labelText = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans).GetComponentInChildren<Text>();
        labelText.text = "Directions";

        // update directions
        for (int i = 0; i < newRecipe.Steps.Length; i++)
        {
            Text infoText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                verticalGroupTrans).GetComponentInChildren<Text>();

            infoText.text = newRecipe.Steps[i];
        }

        #endregion

        #region  Create rating prompt

        Button ratingButton = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans).GetComponentInChildren<Button>();

        ratingButton.enabled = true;
        ratingButton.interactable = true;
        ratingButton.GetComponent<Text>().text = "What did you think?";
        ratingButton.onClick.AddListener(ShowReviewPanel);

        //view reviews
        Text reviewView = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans).GetComponentInChildren<Text>();

        reviewView.text = "Reviews";

        //update reviews
        if(newRecipe.Reviews.Length > 5)
        {
            for (int i = newRecipe.Reviews.Length - 1; i >= newRecipe.Reviews.Length - 5; i--)
            {
                Text reviewText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();

                reviewText.text = newRecipe.Reviews[i];
            }
            Text test = Instantiate(moreReviewsButton, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();
            
        }
        else
        {
            for (int i = newRecipe.Reviews.Length - 1; i >= 0; i--)
            {
                Text reviewText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();

                reviewText.text = newRecipe.Reviews[i];
            }
        }
        

        loadingObject.SetActive(true);
        StartCoroutine(WaitForImage());

        #endregion

        #region Load saved user inputs (if recipe was favorited, rated, reviewed, etc.)

        DatabaseManager.Instance.getFavorites();

        DatabaseManager.Instance
            .GetPreviousSurveyRating(currentRecipe.Key, rating =>
            {
                DrawSurveyRating(rating);
            });
        DatabaseManager.Instance
            .GetCommunityRating(currentRecipe.Key, rating =>
            {
                DrawCommunityRating((int)rating);
            });

        #endregion

        canvas.SetActive(true);
    }

    public void ShowReviewPanel()
    {
        reviewPanel.Reset();
        reviewPanel.recipe = currentRecipe;
        reviewPanel.gameObject.SetActive(true);
    }

    public void HideRewiewPanel()
    {
        reviewPanel.gameObject.SetActive(false);
    }

    public void Enable()
    {
        canvas.SetActive(true);
    }

    public void ShareRecipe()
    {
        RecipeShare.Instance.ShareRecipe(currentRecipe);
    }

    public void Disable()
    {
        canvas.SetActive(false);
    }

    private IEnumerator WaitForImage()
    {
        yield return new WaitWhile(() => dishImage == null);
        loadingObject.SetActive(false);
    }

    public void SetFavorited(List<string> favorites)
    {
        if(favorites.Contains(currentRecipe.Key))
            HandleFavorite();

        else
            HandleUnfavorite();
    }

    public void HandleFavorite()
    {
        bool worked = DatabaseManager.Instance.favoriteRecipe(currentRecipe.Key);

        if (worked)
        {
            unfavoriteButton.SetActive(true);
            //NotificationManager.Instance.ShowNotification("Favorited");
        }
        else
        {
            unfavoriteButton.SetActive(false);
            //NotificationManager.Instance.ShowNotification("Failed to favorite.");
        }
    }

    public void HandleUnfavorite()
    {
        bool worked = DatabaseManager.Instance.unfavoriteRecipe(currentRecipe.Key);
        if (worked)
        {
            unfavoriteButton.SetActive(false);
            //NotificationManager.Instance.ShowNotification("Unfavoriting.");

        }
        else
        {
            //NotificationManager.Instance.ShowNotification("Failed to unfavorite.");
        }
    }

    public void ShowMoreReviews()
    {
        canvas.SetActive(false);
        ReviewManagerUI.Instance.Enable();
    }

    public void Test()
    {
        List<Ingredient> ingredients = new List<Ingredient>();

        for (int i = 0; i < 10; i++)
        {
            ingredients.Add(new Ingredient($"Ingredient {i}", "1/2 cup"));
        }

        List<string> directions = new List<string>();

        for (int i = 0; i < 50; i++)
        {
            directions.Add($"{i}. do the thing");
        }

        List<string> tags = new List<string>() {"Fish"};

        List<string> reviews =
            new List<string>()
            {
                "pretty good",
                "pretty good",
                "pretty good",
                "pretty good",
                "pretty good",
                "pretty good",
                "pretty good",
                "pretty good"
            };

        Recipe recipe = new Recipe("Butter Salmon", "", 560, 45, tags, ingredients, directions, "0", reviews: reviews, starRating: 3);

        InitRecipeUI(recipe);
    }
}
