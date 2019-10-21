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

    private List<GameObject> ingredientObjects = new List<GameObject>();
    private bool ingredientsAreActive = true;
    private List<GameObject> directionsObjects = new List<GameObject>();
    private bool directionsAreActive = true;
    private List<GameObject> reviewsObjects = new List<GameObject>();
    private bool reviewsAreActive = true;

    private List<Review> reviewList = new List<Review>();
    private ReviewController rc;
    private int reviewCounter;

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
        if (gameObject.GetComponent<ReviewController>() == null)
            rc = gameObject.AddComponent<ReviewController>();

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
            for (int i = 0; i < verticalGroupTrans.childCount; i++)
                Destroy(verticalGroupTrans.GetChild(i).gameObject);
        }

        // create ingredients label
        GameObject ingredientLabel = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans);
        ingredientLabel.transform.Find("Button").GetComponentInChildren<Button>().onClick.AddListener(ToggleIngredientState);
        Text labelINText = ingredientLabel.GetComponentInChildren<Text>();

        labelINText.text = "Ingredients";

        // update ingredients
        for (int i = 0; i < newRecipe.Ingredients.Length; i++)
        {
            GameObject ingredientInfo = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                verticalGroupTrans);
            ingredientObjects.Add(ingredientInfo);

            Text infoText = ingredientInfo.GetComponentInChildren<Text>();

            infoText.text = newRecipe.Ingredients[i].ToString();
        }

        #endregion

        #region Load directions

        // create directions label
        GameObject directionLabel = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans);
        directionLabel.transform.Find("Button").GetComponentInChildren<Button>().onClick.AddListener(ToggleDirectionState);
        Text labelDRText = directionLabel.GetComponentInChildren<Text>();

        labelDRText.text = "Directions";

        // update directions
        for (int i = 0; i < newRecipe.Steps.Length; i++)
        {
            GameObject directionInfo = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                verticalGroupTrans);
            directionsObjects.Add(directionInfo);

            Text infoText = directionInfo.GetComponentInChildren<Text>();

            infoText.text = newRecipe.Steps[i];
        }

        #endregion

        #region  Create rating prompt
        GameObject ratingLabel = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans);
        ratingLabel.transform.Find("Button").GetComponentInChildren<Button>().enabled = false;
        ratingLabel.transform.Find("Button").GetComponentInChildren<Button>().image.enabled = false;
        Button ratingButton = ratingLabel.GetComponentInChildren<Button>();

        ratingButton.enabled = true;
        ratingButton.interactable = true;
        ratingButton.GetComponent<Text>().text = "What did you think?";
        ratingButton.onClick.AddListener(ShowReviewPanel);

        //view reviews
        GameObject reviewLabel = Instantiate(labelPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
            verticalGroupTrans);
        reviewLabel.transform.Find("Button").GetComponentInChildren<Button>().onClick.AddListener(ToggleReviewState);

        Text reviewView = reviewLabel.GetComponentInChildren<Text>();

        reviewView.text = "Reviews";

        //update reviews
        rc.getReviews(currentRecipe.Key, HandleReviews);
        


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

    void HandleReviews()
    {
        reviewList = rc.reviewList;
        reviewCounter = reviewList.Count;

        if(reviewCounter == 0)
        {

        }
        else if (reviewCounter > 5)
        {
            for (int i = reviewCounter - 1; i >= (reviewCounter - 5); i--)
            {
                GameObject reviewInfo = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans);
                reviewsObjects.Add(reviewInfo);

                Text reviewText = reviewInfo.GetComponentInChildren<Text>();

                reviewText.text = reviewList[i].content;
            }
            Text test = Instantiate(moreReviewsButton, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();
            test.text = $"Show all {reviewList.Count} reviews";
        }
        else
        {
            for (int i = reviewCounter - 1; i >= 0; i--)
            {
                GameObject reviewInfo = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans);
                reviewsObjects.Add(reviewInfo);

                Text reviewText = reviewInfo.GetComponentInChildren<Text>();

                reviewText.text = reviewList[i].content;
            }
        }
    }

    public void ShowReviewPanel()
    {
        reviewPanel.Reset();
        reviewPanel.recipe = currentRecipe;
        reviewPanel.gameObject.SetActive(true);
        //gameObject.transform.Find("RatingSurveySection").SetAsLastSibling();
    }

    public void HideRewiewPanel()
    {
        reviewPanel.gameObject.SetActive(false);
    }

    public void ToggleIngredientState()
    {
        ingredientsAreActive = !ingredientsAreActive;
        foreach (var item in ingredientObjects)
        {
            item.SetActive(ingredientsAreActive);
        }
    }

    public void ToggleDirectionState()
    {
        directionsAreActive = !directionsAreActive;
        foreach (var item in directionsObjects)
        {
            item.SetActive(directionsAreActive);
        }
    }

    public void ToggleReviewState()
    {
        reviewsAreActive = !reviewsAreActive;
        foreach (var item in reviewsObjects)
        {
            item.SetActive(reviewsAreActive);
        }
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
        ingredientObjects.Clear();
        directionsObjects.Clear();
        reviewsObjects.Clear();
        ingredientsAreActive = true;
        directionsAreActive = true;
        reviewsAreActive = true;
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
        ReviewManagerUI.Instance.InitReviewUI(currentRecipe);
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
