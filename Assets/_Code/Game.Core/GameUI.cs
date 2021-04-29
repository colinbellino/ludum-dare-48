using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class GameUI : MonoBehaviour
	{
		[Header("Debug")]
		[SerializeField] private GameObject _debugRoot;
		[SerializeField] private Text _debugText;
		[Header("Gameplay")]
		[SerializeField] private GameObject _gameplayRoot;
		[Header("Pause")]
		[SerializeField] private GameObject _pauseRoot;
		[SerializeField] public Button PauseButton1;
		[SerializeField] public Button PauseButton2;
		[SerializeField] public Button PauseButton3;
		[SerializeField] public Button PauseButton4;
		[Header("Victory")]
		[SerializeField] private Image _victoryPanel;
		[SerializeField] private TMP_Text _victoryText;
		[SerializeField] public Button VictoryButton1;
		[SerializeField] public Button VictoryButton2;
		[Header("Defeat")]
		[SerializeField] private Image _defeatPanel;
		[SerializeField] private TMP_Text _defeatText;
		[SerializeField] public Button DefeatButton1;
		[SerializeField] public Button DefeatButton2;
		[Header("Transitions")]
		[SerializeField] private Image _fadeToBlackImage;

		private AudioPlayer _audioPlayer;
		private GameConfig _config;

		public void Inject(Game game)
		{
			_audioPlayer = game.AudioPlayer;
			_config = game.Config;
		}

		private void Start()
		{
			HideDebug();
			HideGameplay();
			HidePause();
			_ = HideVictory(0f);
			_ = HideDefeat(0f);

			VictoryButton1.onClick.AddListener(PlayButtonClip);
			VictoryButton2.onClick.AddListener(PlayButtonClip);
			DefeatButton1.onClick.AddListener(PlayButtonClip);
			DefeatButton2.onClick.AddListener(PlayButtonClip);
			PauseButton1.onClick.AddListener(PlayButtonClip);
			PauseButton2.onClick.AddListener(PlayButtonClip);
			PauseButton3.onClick.AddListener(PlayButtonClip);
			PauseButton4.onClick.AddListener(PlayButtonClip);
		}

		private void PlayButtonClip()
		{
			_audioPlayer.PlaySoundEffect(_config.MenuConfirmClip);
		}

		public void ShowDebug() { _debugRoot.SetActive(true); }
		public void HideDebug() { _debugRoot.SetActive(false); }
		public void SetDebugText(string value)
		{
			_debugText.text = value;
		}

		public void ShowGameplay() { _gameplayRoot.SetActive(true); }
		public void HideGameplay() { _gameplayRoot.SetActive(false); }

		public async void ShowPause()
		{
			_pauseRoot.SetActive(true);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(PauseButton1.gameObject);
		}
		public void HidePause() { _pauseRoot.SetActive(false); }

		public async UniTask ShowVictory()
		{
			await FadeInPanel(_victoryPanel, _victoryText, 0.5f);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(VictoryButton1.gameObject);
		}
		public async UniTask HideVictory(float duration = 0.5f)
		{
			await FadeOutPanel(_victoryPanel, duration);
		}

		public async UniTask ShowDefeat()
		{
			await FadeInPanel(_defeatPanel, _defeatText, 0.5f);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(DefeatButton1.gameObject);
		}
		public async UniTask HideDefeat(float duration = 0.5f)
		{
			await FadeOutPanel(_defeatPanel, duration);
		}

		public async UniTask FadeIn(Color color, float duration = 1f)
		{
			await _fadeToBlackImage.DOColor(color, duration);
		}

		public async UniTask FadeOut(float duration = 1f)
		{
			await _fadeToBlackImage.DOColor(Color.clear, duration);
		}

		private async UniTask FadeInPanel(Image panel, TMP_Text text, float duration)
		{
			panel.gameObject.SetActive(true);

			foreach (var t in panel.GetComponentsInChildren<TMP_Text>())
			{
				_ = t.DOFade(1f, 0f);
			}

			_ = panel.DOFade(1f, duration);

			text.maxVisibleCharacters = 0;

			await UniTask.Delay(TimeSpan.FromSeconds(duration));

			_ = _audioPlayer.PlaySoundEffect(_config.MenuTextAppearClip);

			var totalInvisibleCharacters = text.textInfo.characterCount;
			var counter = 0;
			while (true)
			{
				var visibleCount = counter % (totalInvisibleCharacters + 1);
				text.maxVisibleCharacters = visibleCount;

				if (visibleCount >= totalInvisibleCharacters)
				{
					break;
				}

				counter += 1;

				await UniTask.Delay(12);
			}

			foreach (var button in panel.GetComponentsInChildren<Button>())
			{
				_ = button.image.DOFade(1f, duration);
			}
		}

		private async UniTask FadeOutPanel(Image panel, float duration)
		{
			_ = panel.DOFade(0f, duration);

			foreach (var graphic in panel.GetComponentsInChildren<Graphic>())
			{
				_ = graphic.DOFade(0f, duration);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(duration));
			panel.gameObject.SetActive(false);
		}
	}
}
