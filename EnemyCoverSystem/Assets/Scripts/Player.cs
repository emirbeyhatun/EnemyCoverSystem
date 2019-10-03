using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public int hp = 100;
    public Text countDownText;
    public Text winText;
    public GameObject Enemies;
    public BossController Boss;
    private float countDown = 5;
    private bool finishGame = false;
    bool invinsible = false;

    public void Damage (int damage) {
        if (invinsible)
            return;
        hp -= damage;
        if (hp <= 0)
        {
            Destroy(Enemies);
            if(countDownText)
            {
                countDownText.gameObject.SetActive(true);
                countDownText.transform.parent.gameObject.SetActive(true);
            }
            

        }

    }
    private void Update()
    {
        if(countDownText)
        {
            if(countDownText.IsActive() == true)
            {
                countDown -= Time.deltaTime;
                countDownText.text = countDown.ToString();
                if (countDown <= 0)
                {
                    countDownText.gameObject.SetActive(false);
                    countDownText.transform.parent.gameObject.SetActive(false);

                    countDown = 5;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
        if (!finishGame)
        {
            if (Boss)
            {
                if(Boss.hp <= 0)
                {
                    Destroy(Enemies);

                    invinsible = true;
                    finishGame = true;
                    if(winText)
                    {
                        winText.gameObject.SetActive(true);
                        winText.transform.parent.gameObject.SetActive(true);
                    }
                }

            }
            else
            {
                invinsible = true;
                Destroy(Enemies);
                finishGame = true;
                if(winText)
                {
                    winText.gameObject.SetActive(true);
                    winText.transform.parent.gameObject.SetActive(true);
                }
            }

        }
    }
    //// Update is called once per frame
    //void Update () {
    //       if (Input.GetButtonDown("Fire1"))
    //       {

    //           Vector3 dir = transform.parent.GetChild(0).forward;
    //           dir.Normalize();
    //           GameObject clone = Instantiate(bullet, transform.position , Quaternion.identity, null);
    //           clone.transform.rotation = Quaternion.LookRotation(dir);
    //           clone.GetComponent<Rigidbody>().AddForce(dir * bulletSpeed);
    //           Physics.IgnoreCollision(clone.GetComponent<Collider>(), GetComponent<Collider>());
    //           Physics.IgnoreCollision(clone.GetComponent<Collider>(), GetComponentInParent<Collider>());

    //       }
    //}
}
