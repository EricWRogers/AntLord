using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Buildings : MonoBehaviour
{
    public int currentHealth;
    public int TeamID;
    float fallLevel = 10.0f;
    float wiggleAmount = 0.5f;
    public Slider slider;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        slider.value = currentHealth;
        //im not touching the UI rotating with the camera... for now
        if (currentHealth <= 0)
        {
            StartCoroutine(Demolish());
        }
    }
    IEnumerator Demolish()
    {
        float x = gameObject.transform.position.x;
        float z = gameObject.transform.position.z;
        for (int i = 0; i < fallLevel; i++)
        {
            gameObject.transform.position = new Vector3(Random.Range(x - wiggleAmount, x + wiggleAmount),
            gameObject.transform.position.y - 1,
            Random.Range(z - wiggleAmount, z + wiggleAmount));
            yield return new WaitForSeconds(0.5f);
        }
        Destroy(gameObject);

    }
}
