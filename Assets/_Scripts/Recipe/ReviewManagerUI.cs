using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ReviewManagerUI : MonoBehaviour
{
    public static ReviewManagerUI Instance;
    public static Recipe currentRecipe;
    public List<Review> reviewList = new List<Review>();
    public static int reviewCounter;
    [SerializeField] private GameObject canvas;

    [Header("Prefabs")]
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private GameObject infoPrefab;
    [SerializeField] private GameObject moreReviewsButton;

    [SerializeField] private Transform verticalGroupTrans;
    private ReviewController rc;
    private GameObject reviewButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    public void InitReviewUI(Recipe recipe)
    {
        if(gameObject.GetComponent<ReviewController>() == null)
            rc = gameObject.AddComponent<ReviewController>();
        currentRecipe = recipe;

        moreReviewsButton.GetComponent<Button>().transform.SetAsFirstSibling();
        if (verticalGroupTrans.childCount > 1)
        {
            for (int i = 2; i < verticalGroupTrans.childCount; i++)
                Destroy(verticalGroupTrans.GetChild(i).gameObject);
        }
        rc.getReviews(currentRecipe.Key, HandleReviews);
    }

    void HandleReviews()
    {
        reviewList = rc.reviewList;
        reviewCounter = reviewList.Count;

        

        labelPrefab.GetComponentInChildren<Text>().text = "More Reviews";
        labelPrefab.transform.Find("Button").GetComponentInChildren<Button>().enabled = false;
        labelPrefab.transform.Find("Button").GetComponentInChildren<Button>().image.enabled = false;
        
        Text test = moreReviewsButton.GetComponentInChildren<Text>();
        test.text = "Show More Reviews";

        canvas.SetActive(true);

        GenerateReviews();

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

                reviewText.text = reviewList[i].content;
            }
            moreReviewsButton.GetComponentInChildren<Button>().transform.SetAsLastSibling();
            moreReviewsButton.GetComponent<Button>().enabled = false;
            moreReviewsButton.GetComponentInChildren<Text>().text = "No More Reviews";
            reviewCounter = 0;
        }
        else
        {
            for (int i = reviewCounter - 1; i >= (reviewCounter - 5); i--)
            {
                Text reviewText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                    verticalGroupTrans).GetComponentInChildren<Text>();

                reviewText.text = rc.reviewList[i].content;
            }
            reviewCounter -= 5;
            moreReviewsButton.GetComponentInChildren<Button>().transform.SetAsLastSibling();
        }
        
    }

    public void Enable()
    {
        canvas.SetActive(true);
    }

    public void Disable()
    {
        canvas.SetActive(false);
        reviewList.Clear();
        moreReviewsButton.GetComponent<Button>().enabled = true;
        RecipeManagerUI.Instance.Enable();
    }
}
