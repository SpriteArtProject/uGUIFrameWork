using System;
using _FairyExplorer.Scripts;
using uGuiFramework.View;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace uGuiFramework.Component {
    public class FwAnimator : ViewComponentBase {
        [SerializeField] private Animator _animator;
        public Animator animator => _animator;

        public override void Set(IViewData data) {
            _viewData = data;
            var viewData = data as ViewData;

            ResetSubscriptions();

            _subscriptions.Add(viewData?.isVisible?.Subscribe(isVisible => gameObject.SetActive(isVisible)));
            _subscriptions.Add(viewData?.playAnimation?.Where(str => !string.IsNullOrEmpty(str) && _animator.gameObject.activeInHierarchy).Subscribe(SetAnimation));
            _subscriptions.Add(viewData?.replayAnimationTrigger?.Where(isReplay => isReplay).Subscribe(_ => ReplayAnimation()));
            _subscriptions.Add(this.OnEnableAsObservable().Subscribe(_ => SetAnimation(viewData?.playAnimation?.Value)));
        }

        private void SetAnimation(string s) {
            if (!(_viewData is ViewData data)) return;
            _animator.Play(s, endAction: data.playbackAction);
        }

        private void ReplayAnimation() {
            if (!(_viewData is ViewData data)) return;
            _animator.Rebind();
            _animator.Play(data.playAnimation.Value, endAction: data.playbackAction);
            data.replayAnimationTrigger.Value = false;
        }

        public class ViewData : ViewDataBase {
            public readonly ReactiveProperty<string> playAnimation;
            public readonly ReactiveProperty<bool> replayAnimationTrigger;
            public Action playbackAction;

            public ViewData(string playAnimation = "", Action playbackAction = null, bool isVisible = true) : base(isVisible) {
                this.playAnimation = new ReactiveProperty<string> {
                    Value = playAnimation
                };
                replayAnimationTrigger = new ReactiveProperty<bool> {
                    Value = false
                };
                this.playbackAction = playbackAction;
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                playAnimation.Value = data.playAnimation.Value;
                replayAnimationTrigger.Value = data.replayAnimationTrigger.Value;
            }
        }
    }
}