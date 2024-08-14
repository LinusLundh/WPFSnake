using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFSnake
{
    public partial class MainWindow : Window
    {
        private Stopwatch stopwatch = new Stopwatch();
        private const int FrameRate = 15; // Frames per second

        private List<Rectangle> snakeParts = new List<Rectangle>();
        private Point snakeHeadPosition;
        private Vector snakeDirection = new Vector(20, 0);
        private int snakeLength = 5;
        private int score = 0;
        private Random rand = new Random();
        private Rectangle food;
        private const int GridSize = 20;

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += OnKeyDown;
            StartGame();
        }

        private void StartGame()
        {
            snakeParts.Clear();
            GameCanvas.Children.Clear();
            snakeHeadPosition = new Point(100, 100);
            snakeDirection = new Vector(GridSize, 0);
            snakeLength = 5;
            score = 0;
            ScoreText.Text = "Score: 0";
            GameOverText.Visibility = Visibility.Hidden;

            // Initialize snake
            for (int i = 0; i < snakeLength; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = GridSize,
                    Height = GridSize,
                    Fill = Brushes.Green
                };
                snakeParts.Add(rect);
                GameCanvas.Children.Add(rect);
                Canvas.SetLeft(rect, snakeHeadPosition.X - i * GridSize);
                Canvas.SetTop(rect, snakeHeadPosition.Y);
            }

            // Add initial food
            SpawnFood();

            stopwatch.Start();
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds >= 1000 / FrameRate)
            {
                stopwatch.Restart();
                GameTick();
            }
        }

        private void GameTick()
        {
            MoveSnake();
            CheckCollision();
            CheckFoodCollision();
        }

        private void MoveSnake()
        {
            snakeHeadPosition += snakeDirection;

            for (int i = snakeParts.Count - 1; i > 0; i--)
            {
                Canvas.SetLeft(snakeParts[i], Canvas.GetLeft(snakeParts[i - 1]));
                Canvas.SetTop(snakeParts[i], Canvas.GetTop(snakeParts[i - 1]));
            }

            Canvas.SetLeft(snakeParts[0], snakeHeadPosition.X);
            Canvas.SetTop(snakeParts[0], snakeHeadPosition.Y);
        }

        private void CheckCollision()
        {
            // Wrap-around logic
            if (snakeHeadPosition.X < 0)
            {
                snakeHeadPosition.X = GameCanvas.ActualWidth - GridSize;
            }
            else if (snakeHeadPosition.X >= GameCanvas.ActualWidth)
            {
                snakeHeadPosition.X = 0;
            }

            if (snakeHeadPosition.Y < 0)
            {
                snakeHeadPosition.Y = GameCanvas.ActualHeight - GridSize;
            }
            else if (snakeHeadPosition.Y >= GameCanvas.ActualHeight)
            {
                snakeHeadPosition.Y = 0;
            }

            // Check self-collision
            for (int i = 1; i < snakeParts.Count; i++)
            {
                if (snakeHeadPosition.X == Canvas.GetLeft(snakeParts[i]) && snakeHeadPosition.Y == Canvas.GetTop(snakeParts[i]))
                {
                    EndGame();
                }
            }
        }

        private void EndGame()
        {
            stopwatch.Stop();
            CompositionTarget.Rendering -= OnRendering;
            GameOverText.Visibility = Visibility.Visible;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != new Vector(0, GridSize)) // Can't turn down when going up
                        snakeDirection = new Vector(0, -GridSize);
                    break;
                case Key.Down:
                    if (snakeDirection != new Vector(0, -GridSize)) // Can't turn up when going down
                        snakeDirection = new Vector(0, GridSize);
                    break;
                case Key.Left:
                    if (snakeDirection != new Vector(GridSize, 0)) // Can't turn right when going left
                        snakeDirection = new Vector(-GridSize, 0);
                    break;
                case Key.Right:
                    if (snakeDirection != new Vector(-GridSize, 0)) // Can't turn left when going right
                        snakeDirection = new Vector(GridSize, 0);
                    break;
                case Key.R:
                    StartGame();
                    break;
            }
        }

        private void SpawnFood()
        {
            if (food != null)
            {
                GameCanvas.Children.Remove(food);
            }

            food = new Rectangle
            {
                Width = GridSize,
                Height = GridSize,
                Fill = Brushes.Red
            };

            GameCanvas.Children.Add(food);
            PlaceFood();
        }

        private void PlaceFood()
        {
            bool validPosition = false;

            while (!validPosition)
            {
                int maxX = (int)(GameCanvas.ActualWidth / GridSize);
                int maxY = (int)(GameCanvas.ActualHeight / GridSize);

                int foodX = rand.Next(0, maxX) * GridSize;
                int foodY = rand.Next(0, maxY) * GridSize;

                // Ensure food doesn't spawn on the snake's body
                validPosition = true;
                foreach (var part in snakeParts)
                {
                    if (foodX == Canvas.GetLeft(part) && foodY == Canvas.GetTop(part))
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (validPosition)
                {
                    Canvas.SetLeft(food, foodX);
                    Canvas.SetTop(food, foodY);
                }
            }
        }

        private void CheckFoodCollision()
        {
            double foodX = Canvas.GetLeft(food);
            double foodY = Canvas.GetTop(food);
            double headX = snakeHeadPosition.X;
            double headY = snakeHeadPosition.Y;

            // Convert to grid coordinates
            int foodGridX = (int)(foodX / GridSize);
            int foodGridY = (int)(foodY / GridSize);
            int headGridX = (int)(headX / GridSize);
            int headGridY = (int)(headY / GridSize);

            // Check if the snake's head is on the same grid cell as the food
            if (foodGridX == headGridX && foodGridY == headGridY)
            {
                GameCanvas.Children.Remove(food);

                // Increase snake length
                Rectangle newPart = new Rectangle
                {
                    Width = GridSize,
                    Height = GridSize,
                    Fill = Brushes.Green
                };

                snakeParts.Add(newPart);
                GameCanvas.Children.Add(newPart);

                // Increase score
                score += 10;
                ScoreText.Text = $"Score: {score}";

                // Spawn new food
                SpawnFood();
            }
        }
    }
}
