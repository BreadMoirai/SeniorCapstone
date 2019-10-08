using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ReviewManagerUI : MonoBehaviour
{
    public static Recipe thisRecipe;
    public static ReviewManagerUI Instance;
    public static int reviewCounter;
    [SerializeField] private GameObject canvas;

    [Header("Prefabs")]
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private GameObject infoPrefab;
    [SerializeField] private GameObject moreReviewsButton;

    [SerializeField] private Transform verticalGroupTrans;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void InitReviewUI(Recipe newRecipe)
    {
        thisRecipe = newRecipe;
        reviewCounter = thisRecipe.Reviews.Length;
        // remove any previous ingredients and directions
        if (verticalGroupTrans.childCount > 1)
        {
            for (int i = 1; i < verticalGroupTrans.childCount; i++)
                Destroy(verticalGroupTrans.GetChild(i).gameObject);
        }

        labelPrefab.GetComponentInChildren<Text>().text = "More Reviews";

        Text test = Instantiate(moreReviewsButton, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                verticalGroupTrans).GetComponentInChildren<Text>();
        test.text = "Show More Reviews";
        GenerateReviews();
        
        canvas.SetActive(true);
    }

    public void GenerateReviews()
    {
        if (reviewCounter == 0)
        {

        }
        else if (reviewCounter < 5)
        {
            for (int i = reviewCounter - 1; i >= 0; i--)
            {
                Text reviewText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();

                reviewText.text = thisRecipe.Reviews[i];
            }
            Destroy(verticalGroupTrans.GetComponentInChildren<Button>());
            reviewCounter = 0;
        }
        else
        {
            for (int i = reviewCounter - 1; i >= (reviewCounter - 5); i--)
            {
                Text reviewText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();

                reviewText.text = thisRecipe.Reviews[i];
            }
            reviewCounter -= 5;
        }
        verticalGroupTrans.GetComponentInChildren<Button>().transform.SetAsLastSibling();
    }

    public void Enable()
    {
        canvas.SetActive(true);
    }

    public void Disable()
    {
        canvas.SetActive(false);
        RecipeManagerUI.Instance.Enable();
    }
}
