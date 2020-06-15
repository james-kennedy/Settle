using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class overall_flow : MonoBehaviour
{
    const int MAX_PEOPLE = 8;

    GameObject panel_one;
    GameObject panel_two;

    // first screen objects
    public Slider sldTip;
    public Slider sldPeople;
    public InputField txtSubtotal;
    public InputField txtTax;
    GameObject btnNext;

    // vars to store slider final input
    private int iTipPercent;

    // vars for bill amounts
    private decimal decSubtotal;
    private decimal decTax;
    private decimal decGrandTotal;
    private decimal decTipTotal;
    private decimal decAssignedTotal;
    private decimal decTaxPercent;

    private string strCurrentItem;  // store as a string then convert to decimal when needed (easier concatenation)
    Dictionary<string, decimal> dictButtonItemTotal = new Dictionary<string, decimal>(MAX_PEOPLE);

    // second screen objects
    GameObject lblRemaining;
    GameObject txtCalcTip;
    GameObject txtCalcTotal;
    GameObject lblItem;


    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = false;  // don't go fullscreen on mobile

        // get handles on all the game objects that aren't passed in the editor
        panel_one = GameObject.Find("pnl_one");
        panel_two = GameObject.Find("pnl_two");

        btnNext = GameObject.Find("btn_next");
        btnNext.GetComponentInChildren<Text>().text = ">>";

        LoadKeypadButtons();
        LoadPersonButtons();

        lblItem = GameObject.Find("lbl_item");
        lblRemaining = GameObject.Find("lbl_remaining");
        txtCalcTip = GameObject.Find("txt_calc_tip");
        txtCalcTotal = GameObject.Find("txt_calc_total");

        strCurrentItem = "";
        decAssignedTotal = 0.0m;

        panel_one.SetActive(true);      // show first screen
        panel_two.SetActive(false);     // hide second screen
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadKeypadButtons()
    {
        // programmatically get all the number buttons and set their text
        for (int i = 0; i <= 9; i++)
        {
            string strThisBtn = "btn_num" + i.ToString();
            GameObject this_btn = GameObject.Find(strThisBtn);
            this_btn.GetComponentInChildren<Text>().text = i.ToString();
        }
    }

    void LoadPersonButtons()
    {
        // loop and load person button labels, add decimal to the class dict to keep a running total of items
        for(int i = 1; i <= MAX_PEOPLE; i++)
        {
            string strThisBtn = "btn_pers" + i.ToString();
            GameObject this_btn = GameObject.Find(strThisBtn);
            this_btn.GetComponentInChildren<Text>().text = "0.00";
            this_btn.GetComponentInChildren<Text>().fontSize = 24;
            dictButtonItemTotal[strThisBtn] = 0.0m;
        }
    }

    public void NextScreen()
    {
        // make sure that both subtotal and tax are valid, get them into decimal
        bool canConvertSub = false;
        bool canConvertTax = false;

        string strSubtotal = txtSubtotal.text;
        string strTax = txtTax.text;

        canConvertSub = decimal.TryParse(strSubtotal, out decSubtotal);
        canConvertTax = decimal.TryParse(strTax, out decTax);

        if (canConvertSub == true && canConvertTax == true)
        {
            // hide the first screen
            panel_one.SetActive(false);

            // get slider values
            iTipPercent = (int)sldTip.value;
            int iPeople = (int)sldPeople.value;

            // do the maths for the totals
            decTipTotal = RoundUp((decSubtotal / 100) * iTipPercent, 2);
            decGrandTotal = decSubtotal + decTipTotal + decTax;
            decTaxPercent = decTax / decSubtotal;

            lblRemaining.GetComponentInChildren<Text>().text = strSubtotal;

            txtCalcTip.GetComponentInChildren<Text>().text = decTipTotal.ToString("F2");
            txtCalcTotal.GetComponentInChildren<Text>().text = decGrandTotal.ToString("F2");

            // show the second screen
            panel_two.SetActive(true);

            // remove people from view (has to be done after panel 2 is active, or null ref exception)
            HidePeople(iPeople);
        }
    }

    private void HidePeople(int iPeople)
    {
        // loop through, find the buttons for each people and hide out of range ones
        for (int i = (iPeople+1); i <= MAX_PEOPLE; i++)
        {
            string strThisBtn = "btn_pers" + i.ToString();
            GameObject this_btn = GameObject.Find(strThisBtn);
            this_btn.SetActive(false);
        }
    }

    public static decimal RoundUp(decimal input, int places)
    {
        // round a decimal to a given number of decimal places
        double multiplier = Math.Pow(10, Convert.ToDouble(places));
        return (decimal)(Math.Ceiling((double)input * multiplier) / multiplier);
    }

    public void AssignToPerson(Button button)
    {
        // check that the item has a value
        if (strCurrentItem.Length < 1) 
            return;

        // check that the item doesn't take the remaining below 0
        string strFormattedItem = FormattedNumber(strCurrentItem);
        decimal decItem = Convert.ToDecimal(strFormattedItem);
        decimal decNewRemaining = decSubtotal - decAssignedTotal - decItem;

        if (decNewRemaining < 0)
            return;

        // add to person's list and assigned list
        dictButtonItemTotal[button.name] += decItem;
        decAssignedTotal += decItem;

        // update person's owings (including recalc of tax + tip) and remaining label
        decimal decThisOwing = dictButtonItemTotal[button.name] + ((dictButtonItemTotal[button.name] / 100) * iTipPercent) + (dictButtonItemTotal[button.name] * decTaxPercent);
        button.GetComponentInChildren<Text>().text = RoundUp(decThisOwing, 2).ToString("F2");
        lblRemaining.GetComponentInChildren<Text>().text = decNewRemaining.ToString("F2");

        // set item value back to 0.0
        strCurrentItem = "";
        lblItem.GetComponentInChildren<Text>().text = "0.00";
    }

    public void PressedNumber(Button button)
    {
        // use button name to figure out what was pressed
        string strButtonName = button.name;
        string strNumber = strButtonName.Substring(strButtonName.Length - 1, 1);

        // add to running total
        strCurrentItem += strNumber;

        // update display
        lblItem.GetComponentInChildren<Text>().text = FormattedNumber(strCurrentItem);
    }

    public void DeleteNumber()
    {
        if (strCurrentItem.Length > 0)
        {
            strCurrentItem = strCurrentItem.Substring(0, strCurrentItem.Length - 1);
            lblItem.GetComponentInChildren<Text>().text = FormattedNumber(strCurrentItem);
        }
    }

    private string FormattedNumber(string strNumIn) 
    {
        if (strNumIn.Length < 1)
        {
            return "0.00";
        }
        else if (strNumIn.Length < 2)
        {
            return "0.0" + strNumIn;
        }
        else if (strNumIn.Length < 3)
        {
            return "0." + strNumIn;
        }
        else
        {
            return strNumIn.Substring(0, strNumIn.Length - 2) + "." + strNumIn.Substring(strNumIn.Length - 2, 2);
        }
    }

}
