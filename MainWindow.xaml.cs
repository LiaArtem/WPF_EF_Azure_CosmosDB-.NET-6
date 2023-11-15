using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Design;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Windows.Shapes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using System.Runtime.ConstrainedExecution;
using static System.Net.Mime.MediaTypeNames;

namespace WPF_EF_Azure_CosmosDB
{    
    public class UserData
    {        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Id { get; set; }        
        private string? textValue;
        private int? intValue;
        private double? doubleValue;
        private Boolean? boolValue;
        private DateTime? dateValue;        
        public string? TextValue
        {
            get { return textValue; }
            set { textValue = value; OnPropertyChanged("TextValue"); }
        }
        public int? IntValue
        {
            get { return intValue; }
            set { intValue = value; OnPropertyChanged("IntValue"); }
        }
        public double? DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; OnPropertyChanged("DoubleValue"); }
        }
        public Boolean? BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; OnPropertyChanged("BoolValue"); }
        }
        public DateTime? DateValue
        {
            get { return dateValue; }
            set { dateValue = value; OnPropertyChanged("DateValue"); }
        }        

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }    

    public class ApplicationContext : DbContext
    {
        public DbSet<UserData> UsersData { get; set; }
        
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {            
            Database.EnsureCreated();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        readonly bool is_initialize = true;
        bool is_filter = false;

        public MainWindow()
        {
            InitializeComponent();
            
            is_initialize = false;            
            using ApplicationContext db = new(LoadConfiguration());
            UpdateDatagrid(db);            
        }

        // загрузить NuGet
        // Microsoft.Extensions.Configuration.FileExtensions;
        // Microsoft.Extensions.Configuration.Json;
        public static DbContextOptions<ApplicationContext> LoadConfiguration()
        {
            var builder = new ConfigurationBuilder() 
                                    .SetBasePath(Directory.GetCurrentDirectory()) // установка пути к текущему каталогу
                                    .AddJsonFile("appsettings.json"); // получаем конфигурацию из файла appsettings.json                                    
            // создаем конфигурацию
            var config = builder.Build();            
            
            // строка подключения
            string? connectionString = config.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

            string m_AccountEndpoint = "", m_AccountKey = "";
            foreach (string? part in connectionString!.Split(";"))
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (part.Contains("AccountEndpoint=")) { m_AccountEndpoint = part[(part.IndexOf(Convert.ToChar("=")) + 1)..]; }
                if (part.Contains("AccountKey=")) { m_AccountKey = part[(part.IndexOf(Convert.ToChar("=")) + 1)..]; }
            }
            
            var options = optionsBuilder
                .UseCosmos(m_AccountEndpoint, m_AccountKey, "UserData")
                .UseLoggerFactory(MyLoggerFactory)
                .Options;

            return options;
        }

        // устанавливаем фабрику логгера
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            //Настройка логгирования (свой провайдер)
            //Логгирование позволяет получить разнообразную информацию, связанную с операциями с данными, но не вся эта информация может быть нам нужна.Для фильтрации этой информации EF Core предоставляет класс DbLoggerCategory, который позволяет задать нужные категории логгирования:
            //Database.Command: категория для выполняемых команд, позволяет получить выполняемый код SQL
            //Database.Connection : категория для операций подключения к БД
            //Database.Transaction : категория для транзакций с бд
            //Database.Migration: категория для миграций
            //Database.Model: категория для действий, совершаемых при привязке модели
            //Database.Query: категория для запросов за исключением тех, что генерируют исполняемый код SQL
            //Database.Scaffolding: категория для действий, выполняемых в поцессе обратного инжиниринга(то есть когда по базе данных генерируются классы и класс контекста)
            //Database.Update: категория для сообщений вызова DbContext.SaveChanges()
            //Database.Infrastructure: категория для всех остальных сообщений
            
            builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name
                                    && level == LogLevel.Information)
                    .AddProvider(new MyLoggerProvider()); // указываем наш провайдер логгирования
            

            // или стандартный от Microsoft
            // NuGet - Microsoft.Extensions.Logging.Console
            //builder.AddConsole();
        });

        private void UpdateDatagrid(ApplicationContext db)                
        {
            if (is_initialize == true) return;
            if (is_filter == false)
            {
                DataGrid1.ItemsSource = db.UsersData.ToList();                
            }
            else
            {
                string m_value1 = value1.Text.ToString();
                String m_value2 = value2.Text.ToString();
                int m_value1_int; int m_value2_int;
                bool m_value1_bool;
                bool m_er;

                if (value_type.Text == "id")
                {
                    m_er = int.TryParse(m_value1, out m_value1_int);
                    m_er = int.TryParse(m_value2, out m_value2_int);
                    DataGrid1.ItemsSource = db.UsersData.ToList().Where(p => EF.Functions.Like(p.Id, "%" + m_value1 + "%"));
                }
                else if (value_type.Text == "text")
                {
                    DataGrid1.ItemsSource = db.UsersData.ToList().Where(p => EF.Functions.Like(p.TextValue ?? "", "%" + m_value1 + "%"));
                }
                else if (value_type.Text == "int")
                {
                    m_er = int.TryParse(m_value1, out m_value1_int);
                    m_er = int.TryParse(m_value2, out m_value2_int);
                    DataGrid1.ItemsSource = db.UsersData.ToList().Where(p => p.IntValue >= m_value1_int && p.IntValue <= m_value2_int);
                }
                else if (value_type.Text == "double")
                {
                    m_er = double.TryParse(m_value1, out double m_value1_dbl);
                    m_er = double.TryParse(m_value2, out double m_value2_dbl);
                    DataGrid1.ItemsSource = db.UsersData.ToList().Where(p => p.DoubleValue >= m_value1_dbl && p.DoubleValue <= m_value2_dbl);
                }
                else if (value_type.Text == "bool")
                {
                    m_value1_bool = false;
                    if (m_value1.Equals("T", StringComparison.CurrentCultureIgnoreCase) || m_value1.Equals("true", StringComparison.CurrentCultureIgnoreCase)) m_value1_bool = true;
                    DataGrid1.ItemsSource = db.UsersData.ToList().Where(p => p.BoolValue == m_value1_bool);
                }
                else if (value_type.Text == "date")
                {
                    m_er = DateTime.TryParseExact(m_value1, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value1_dat);
                    m_er = DateTime.TryParseExact(m_value2, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value2_dat);
                    DataGrid1.ItemsSource = db.UsersData.ToList().Where(p => p.DateValue >= m_value1_dat && p.DateValue <= m_value2_dat);
                }                
            }
            this.DataContext = DataGrid1.ItemsSource; //db.UsersData.ToList();

            // Выделить сроку с курсором
            if (DataGrig_Id == null && DataGrid1.Items.Count > 0) DataGrig_Id = "1";

            if (DataGrig_Id != null && DataGrid1.Items.Count > 0) 
            {
                foreach (UserData drv in DataGrid1.ItemsSource)
                {
                    if ( drv.Id == DataGrig_Id)
                    {
                        DataGrid1.SelectedItem = drv;
                        DataGrid1.ScrollIntoView(drv);
                        DataGrid1.Focus();
                        break;
                    }
                }             
            }           
        }
        
        // добавить запись
        private void Button_insertClick(object sender, RoutedEventArgs e)
        {
            AddWindow addWin = new(new UserData());
            if (addWin.ShowDialog() == true)
            {                
                UserData ud = addWin.UserDataAdd;

                var cur = Mouse.OverrideCursor;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                
                using ApplicationContext db = new(LoadConfiguration());
                db.UsersData.Add(ud);
                db.SaveChanges();
                UpdateDatagrid(db);

                Mouse.OverrideCursor = cur;
            }            
        }

        // изменить запись
        private void Button_updateClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;
            // получаем выделенный объект
            if (DataGrid1.SelectedItem is not UserData ud) { return; }

            AddWindow addWin = new(new UserData
            {
                Id = ud.Id,
                TextValue = ud.TextValue,
                IntValue = ud.IntValue,
                DoubleValue = ud.DoubleValue,
                BoolValue = ud.BoolValue,
                DateValue = ud.DateValue                
            }); 

            if (addWin.ShowDialog() == true)
            {
                // получаем измененный объект                
                using ApplicationContext db = new(LoadConfiguration());
                ud = db.UsersData.Find(addWin.UserDataAdd.Id)!;
                if (ud != null)
                {
                    ud.TextValue = addWin.UserDataAdd.TextValue;
                    ud.IntValue = addWin.UserDataAdd.IntValue;
                    ud.DoubleValue = addWin.UserDataAdd.DoubleValue;
                    ud.BoolValue = addWin.UserDataAdd.BoolValue;
                    ud.DateValue = addWin.UserDataAdd.DateValue;

                    try
                    {
                        var cur = Mouse.OverrideCursor;
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                        db.Entry(ud).State = EntityState.Modified;
                        db.SaveChanges();
                        UpdateDatagrid(db);

                        Mouse.OverrideCursor = cur;

                        MessageBox("Запись обновлена");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        MessageBox("Запись заблокирована другим пользователем", System.Windows.MessageBoxImage.Warning);
                        UpdateDatagrid(db);                        
                    }
                }
            }            
        }

        // удалить запись
        private void Button_deleteClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;

            MessageBoxResult result = System.Windows.MessageBox.Show("Удалить запись ???", "Сообщение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // получаем выделенный объект
                    if (DataGrid1.SelectedItem is not UserData ud) { return; }
                    using (ApplicationContext db = new(LoadConfiguration()))
                    {
                        var cur = Mouse.OverrideCursor;
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                        db.UsersData.Remove(ud);
                        db.SaveChanges();
                        UpdateDatagrid(db);

                        Mouse.OverrideCursor = cur;
                    }
                    MessageBox("Запись удалена");                    
                    break;
                case MessageBoxResult.No:                    
                    break;
            }
        }

        // обновить запись
        private void Button_selectClick(object sender, RoutedEventArgs e)
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            
            using ApplicationContext db = new(LoadConfiguration());
            UpdateDatagrid(db);

            Mouse.OverrideCursor = cur;
        }

        private readonly SolidColorBrush hb = new(Colors.MistyRose);
        private readonly SolidColorBrush nb = new(Colors.AliceBlue);
        private void DataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //UserData product = (UserData) e.Row.DataContext;
            //if (product.Id % 2 == 0)
            if ((e.Row.GetIndex() + 1) % 2 == 0)
                e.Row.Background = hb;
            else
                e.Row.Background = nb;

            // А можно в WPF установить - RowBackground - для нечетных строк и AlternatingRowBackground
        }

        // вывод диалогового окна
        public static void MessageBox(String infoMessage, MessageBoxImage mImage = System.Windows.MessageBoxImage.Information)
        {
            System.Windows.MessageBox.Show(infoMessage, "Сообщение", System.Windows.MessageBoxButton.OK, mImage);
        }

        public string? DataGrig_Id;
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row_list = (UserData)DataGrid1.SelectedItem;
                if (row_list != null)  
                      DataGrig_Id = row_list.Id;                
            }
            catch {
                DataGrig_Id = null;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button_updateClick(sender, e);            
        }

        // применить фильтр
        private void Button_findClick(object sender, RoutedEventArgs e)
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            is_filter = true;            
            using ApplicationContext db = new(LoadConfiguration());
            UpdateDatagrid(db);

            Mouse.OverrideCursor = cur;
        }

        // отменить фильтр
        private void Button_find_cancelClick(object sender, RoutedEventArgs e)
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            is_filter = false;
            value1.Text = "";
            value2.Text = "";            
            using ApplicationContext db = new(LoadConfiguration());
            UpdateDatagrid(db);

            Mouse.OverrideCursor = cur;
        }      

        // изменение типа данных
        private void Value_type_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_initialize == true) return;

            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;            
            String? value_type = selectedItem.Content.ToString();

            if (value_type == "id") value2.IsEnabled = true;
            else if (value_type == "text") {value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "int") value2.IsEnabled = true;
            else if (value_type == "double") value2.IsEnabled = true;
            else if (value_type == "bool") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "date") value2.IsEnabled = true;            
        }

        // изменение фокуса на value2
        private void Value2_GotKeyboardFocus(object sender, EventArgs e)
        {
            if (value1.Text != "") value2.Text = value1.Text;
        }        

        // логирование действий с базой данных -> log.txt (свой провайдер)        
        public class MyLoggerProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName)
            {
                return new MyLogger();
            }

            public void Dispose() { GC.SuppressFinalize(this); }

            private class MyLogger : ILogger
            {
                public IDisposable BeginScope<TState>(TState state) => default!;
                public bool IsEnabled(LogLevel logLevel) => true;

                //BeginScope: этот метод возвращает объект IDisposable, который представляет некоторую область видимости для логгера.В данном случае нам этот метод не важен, поэтому возвращаем значение null
                //IsEnabled: возвращает значения true или false, которые указывает, доступен ли логгер для использования.Здесь можно здать различную логику. В частности, в этот метод передается объект LogLevel, и мы можем, к примеру, задействовать логгер в зависимости от значения этого объекта. Но в данном случае просто возвращаем true, то есть логгер доступен всегда.
                //Log: этот метод предназначен для выполнения логгирования. Он принимает пять параметров:
                //LogLevel: уровень детализации текущего сообщения
                //EventId: идентификатор события
                //TState: некоторый объект состояния, который хранит сообщение
                //Exception: информация об исключении
                //formatter: функция форматирования, которая с помощью двух предыдущих параметров позволяет получить собственно сообщение для логгирования               
                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                    File.AppendAllText("log.txt", formatter(state, exception));
                    Console.WriteLine(formatter(state, exception));
                }
            }
        }
    }
}