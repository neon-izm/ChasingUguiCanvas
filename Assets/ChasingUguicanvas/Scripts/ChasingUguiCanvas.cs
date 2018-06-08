using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRuGUIUtility
{
    /// <summary>
    /// worldに張り付いているキャンバスだけど見えなくなったら追従してくれるやつ
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ChasingUguiCanvas : MonoBehaviour
    {
        [SerializeField] [Tooltip("by default, this camera find main camera ")]
        private Camera _targetCamera;

        private Canvas _canvas;

        private Vector3 _canvasWorldSize = Vector3.zero;

        [SerializeField] private float _chasingTimeSeconds = 1.3f;

        private float _cameraToCanvasDistance = 0f;

        private bool _isChasing = false;

        private Vector3 _defaultcanvasEulerAngle = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }

            _canvas = GetComponent<Canvas>();
            RectTransform rectTransform = _canvas.GetComponent<RectTransform>();
            _canvasWorldSize = new Vector3(rectTransform.sizeDelta.x * rectTransform.localScale.x,
                rectTransform.sizeDelta.y * rectTransform.localScale.y, 0.1f);

            //Set Threashould
            _canvasWorldSize *= 0.4f;

            //get default distance
            _cameraToCanvasDistance = Vector3.Distance(_canvas.transform.position, _targetCamera.transform.position);

            //TODO:
            //get default angle maybe 0,0,0


            Debug.Log("Canvas Size:" + _canvasWorldSize);
        }


        /// <summary>
        /// カメラの四錐体に対してこのCanvasが見えているか（見切れているか）を判定する
        /// </summary>
        /// <returns></returns>
        private bool CanvasCanShownByCamera()
        {
            // 視錐台の6平面を取得
            Plane[] planes =
                GeometryUtility.CalculateFrustumPlanes(_targetCamera.projectionMatrix *
                                                       _targetCamera.worldToCameraMatrix);

            Bounds bounds = new Bounds(transform.position, _canvasWorldSize);
            // 内外判定
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }

        /// <summary>
        /// Tween抑制フラグを解除する。Invokeによって時限発火して呼ばれる。
        /// </summary>
        private void StopChasing()
        {
            _isChasing = false;
        }

        void Update()
        {
            //カメラからcanvasが見えないなら
            if (CanvasCanShownByCamera() == false && _isChasing == false)
            {
                //位置と向きをうまいこと合わせる
                iTween.MoveTo(gameObject,
                    _targetCamera.transform.position + _targetCamera.transform.forward * _cameraToCanvasDistance,
                    _chasingTimeSeconds);
                iTween.RotateTo(gameObject,_targetCamera.transform.rotation.eulerAngles,_chasingTimeSeconds);
                
                //Tween中の連続起動を抑制して
                _isChasing = true;
                //時間が来たら抑制フラグを戻す。まあiTweenのonCompleteで呼んでも良いんですが、このくらいの処理ならInvoke一発でしょ…
                Invoke("StopChasing", _chasingTimeSeconds);
            }
        }
    }
}