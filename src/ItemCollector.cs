using UnityEngine;
using TMPro;

public class ItemCollector : MonoBehaviour
{
    private int gems = 0;

    [SerializeField] private TMP_Text gemsText; 
    [SerializeField] private AudioSource pickupSound;  // AudioSource for gem pickup sound

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Gem"))
        {
            Destroy(collision.gameObject);
            gems++;
            gemsText.text = "Gems: " + gems;  // Update gem count using TextElement
            pickupSound.Play();  // Play the pickup sound

        }
    }
}
