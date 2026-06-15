using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicas;

    private int indiceAtual = 0;

    void Start()
    {
        if (musicas.Length > 0)
        {
            audioSource.clip = musicas[indiceAtual];
            audioSource.Play();
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            ProximaMusica();
        }
    }

    void ProximaMusica()
    {
        indiceAtual++;

        if (indiceAtual >= musicas.Length)
        {
            indiceAtual = 0; // volta para a primeira
        }

        audioSource.clip = musicas[indiceAtual];
        audioSource.Play();
    }
}