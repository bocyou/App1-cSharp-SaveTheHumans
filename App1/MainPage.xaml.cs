using App1.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Xaml.Media.Animation;
// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234237 上有介绍

namespace App1
{
    /// <summary>
    /// 基本页，提供大多数应用程序通用的特性。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Random random = new Random();//This is called a field
        DispatcherTimer enemyTimer = new DispatcherTimer();
        DispatcherTimer targetTimer =new DispatcherTimer();
        bool humanCaptured=false;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper 在每页上用于协助导航和
        /// 进程生命期管理
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();

            enemyTimer.Tick += enemyTimer_Tick;
            enemyTimer.Interval = TimeSpan.FromSeconds(2);

            targetTimer.Tick += targetTimer_Tick;
            targetTimer.Interval = TimeSpan.FromSeconds(.1);
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        void targetTimer_Tick(object sender, object e)
        {
            progressBar.Value += 1;
            if (progressBar.Value >= progressBar.Maximum)
                EndTheGame();
        }

        private void EndTheGame()
        {
            if(!playArea.Children.Contains(GameOverText)){
                enemyTimer.Stop();
                targetTimer.Stop();
                humanCaptured=false;
                startButton.Visibility=Visibility.Visible;
                playArea.Children.Add(GameOverText);    
            }
        }

        void enemyTimer_Tick(object sender, object e)
        {
            AddEnemy();
        }

        /// <summary>
        ///使用在导航过程中传递的内容填充页。 在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 的字典。 首次访问页面时，该状态将为 null。</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。  值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        ///<param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/></param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper 注册

        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// 
        /// 应将页面特有的逻辑放入用于
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 和 <see cref="GridCS.Common.NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            human.IsHitTestVisible = true;
            humanCaptured = false;
            progressBar.Value = 0;
            startButton.Visibility = Visibility.Collapsed;
            playArea.Children.Clear();
            playArea.Children.Add(target);
            playArea.Children.Add(human);
            enemyTimer.Start();
            targetTimer.Start();
        }

        private void AddEnemy()
        {
            ContentControl enemy =new ContentControl();
            enemy.Template = Resources["EnemTemplate"] as ControlTemplate;
            AnimateEnemy(enemy,0, playArea.ActualWidth-100,"(Canvas.Left)");
            AnimateEnemy(enemy,random.Next((int)playArea.ActualWidth-100),random.Next((int)playArea.ActualHeight-100),"(Canvas.Top)");
            playArea.Children.Add(enemy);
            enemy.PointerEntered+=enemy_PointerEntered;//???
        }

        private void enemy_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (humanCaptured)
                EndTheGame();
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard()
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void human_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (enemyTimer.IsEnabled)
            {
                humanCaptured = true;
                human.IsHitTestVisible = false;
            }
        }

        private void target1_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (targetTimer.IsEnabled && humanCaptured)
            {
                progressBar.Value = 0;
                Canvas.SetLeft(target, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(target,random.Next(100,(int)playArea.ActualHeight-100));
                Canvas.SetLeft(human,random.Next(100,(int)playArea.ActualWidth-100));
                Canvas.SetTop(human,random.Next(100,(int)playArea.ActualHeight-100));
                humanCaptured=false;
                human.IsHitTestVisible=true;
            }
        }

        private void playArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (humanCaptured)
            {
                Point pointerPosition = e.GetCurrentPoint(null).Position;
                Point relativePosition = grid.TransformToVisual(playArea).TransformPoint(pointerPosition);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualHeight * 3)||(Math.Abs(relativePosition.Y-Canvas.GetTop(human))>human.ActualHeight*3))
                {
                    humanCaptured = false;
                    human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 2);
                }
            }
        }

        private void playArea_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (humanCaptured)
                EndTheGame();
        }
    }
}