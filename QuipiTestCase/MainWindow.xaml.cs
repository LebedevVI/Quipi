/* Необходимо разработать клиентское приложение, которое:
       Читает из файла список Url
       Загружает соответствующие html страницы по Url
       Находит на страницах все тэги <a> и считает их количество
       После завершения обработки выводит список прочитанных Url  и количество тэгов <a>
    Обязательные требования:
        Приложение должно быть написано на WPF
        Приложение должно поддерживать запуск и отмену операции подсчёта количества тэгов
        Приложение должно оставаться отзывчивым во время работы. Оно должно каким-либо способом показывать пользователю о том, что процесс выполняется
        Приложение должно каким-либо образом визуально выделить тот Url, по которому было насчитано наибольшее количество тэгов
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using mshtml;
using Microsoft.Win32;

namespace QuipuTestCase
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            urlButton.Click += ButtonClick;
            cancelButton.Click += CancelClick;
            htmlLoader.LoadCompleted += GoNextPage;
        }
        /// <summary>
        /// Искомый тэг.
        /// </summary>
        private string findeTag = "<a";
        /// <summary>
        /// Адрес максимального нахождения тэга.
        /// </summary>
        private string maxUrl = "";
        /// <summary>
        /// Максимальное количество искомого тэга на странице.
        /// </summary>
        private int maxValue = 0;
        /// <summary>
        /// Загружаемые из файла Url.
        /// </summary>
        private Stack<Uri> pageStack = new Stack<Uri>();
        /// <summary>
        /// Флаг запроса пользователем отмены действия.
        /// </summary>
        private bool cancelFlag = false;
        /// <summary>
        /// Просмотр страницы и подсчет искомого тэга.
        /// </summary>
        private void PageCheck()
        {
            try
            {
                var l_document = (HTMLDocument)htmlLoader.Document;
                var l_htmlText = l_document.documentElement.innerHTML;
                var l_value = l_htmlText.Split(new string[] { findeTag }, StringSplitOptions.None).Count() - 1;
                if (l_value > maxValue)
                {
                    maxUrl = htmlLoader.Source.ToString();
                    maxValue = l_value;
                }
                resultList.Items.Add(htmlLoader.Source.ToString() + " " + l_value);
            }
            catch
            {
                resultList.Items.Add("Ошибка в обработке страницы");
            }
        }
        /// <summary>
        /// Обработка события завершения загрузки страницы в htmlLoader.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoNextPage(object sender, NavigationEventArgs e)
        {
            if (pageStack.Count > 0 && !cancelFlag)
            {
                htmlLoader.Navigate(pageStack.Pop());
                progressBar.Value++;
                PageCheck();
            }
            else
            {
                urlButton.IsEnabled = true;
                cancelButton.IsEnabled = false;
                progressBar.Visibility = Visibility.Hidden;
                PageCheck();
                resultList.Items.Add("Максимальное значение " + maxUrl + " " + maxValue);
            }
        }
        /// <summary>
        /// Обработка события нажатия на кнопку "отмена".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            cancelFlag = true;
        }
        /// <summary>
        /// Обработка события нажания на кнопку загрузки файла.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string l_path = openFileDialog.FileName;
                resultList.Items.Clear();
                cancelFlag = false;
                try
                {
                    using (StreamReader sr = new StreamReader(l_path, System.Text.Encoding.Default))
                    {
                        string l_line;
                        while ((l_line = sr.ReadLine()) != null)
                        {
                            try
                            {
                                var l_uri = new Uri(l_line);
                                pageStack.Push(l_uri);
                            }
                            catch
                            { }
                        }
                        if (pageStack.Count > 0)
                        {
                            urlButton.IsEnabled = false;
                            cancelButton.IsEnabled = true;
                            progressBar.Visibility = Visibility.Visible;
                            progressBar.Maximum = pageStack.Count();
                            progressBar.Value = 0;
                            htmlLoader.Navigate(pageStack.Pop());
                        }
                        else
                        {
                            resultList.Items.Add("Указанный файл не содержит корректных Url");
                        }
                    }
                }
                catch
                {
                    resultList.Items.Add("Ошибка в работе с файлом");
                }
            }
        }
    }
}
