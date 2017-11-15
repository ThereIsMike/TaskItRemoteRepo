﻿using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class MainViewModel : ViewModelBase
    {
        public IReactiveProperty<int> NumberofItems { get; set; } = new ReactiveProperty<int>();

        public IReactiveProperty<int> NumberofUsers { get; set; } = new ReactiveProperty<int>();
        public ObservableCollection<Products> List { get; set; } = new ObservableCollection<Products>();

        public TrulyObservableCollection<ProductsShow> ListShow { get; set; } = new TrulyObservableCollection<ProductsShow>();

        public ObservableCollection<Buyers> UserList { get; set; } = new ObservableCollection<Buyers>();

        public ReactiveProperty<bool> PushProduct { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<bool> ProductUpdated { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> ProductName { get; set; } = new ReactiveProperty<string>("Empty");

        public ReactiveProperty<Buyers> SelectedBuyer { get; set; } = new ReactiveProperty<Buyers>();


        public MainViewModel()
        {
            Subscriptions();
            UpdateLists();
            
        }

        void Subscriptions()
        {
            

            this.PushProduct.Subscribe(x => 
            { if (x)
                {
                    Console.WriteLine(this.ProductName.Value.ToString());
                    using (var db = new ShoppingContext())
                    {
                        db.ProductAction.Add(new Products() { Name = this.ProductName.Value.ToString() });
                        db.SaveChanges();
                    }
                    this.ProductName.Value = "";
                   this.ProductUpdated.Value = true;
                }
            });

            Observable.Interval(TimeSpan.FromSeconds(0.5)).ObserveOnDispatcher().Subscribe(_ =>
            {
                if (this.ProductUpdated.Value)
                {
                    this.PushProduct.Value = !this.PushProduct.Value;
                    this.ProductUpdated.Value = false;

                        UpdateLists();
                  
                }
            });

            this.SelectedBuyer.Skip(1).Select(x => (new ShoppingContext()).BuyersAction.Where(y => y.FirstName == x.FirstName)).Subscribe(z =>  Console.WriteLine(z.First().BuyerId.ToString()));

            this.ListShow.CollectionChanged += MyItemsSource_CollectionChanged;
            this.ListShow.CollectionChanged += (this.CollectionChangedMethod);
        }
        void MyItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("Changed");
        }
        private void CollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {
            //different kind of changes that may have occurred in collection
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Console.WriteLine("Add");
            }
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                Console.WriteLine("Replace");
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Console.WriteLine("Remove");
            }
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                Console.WriteLine("Move");
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Console.WriteLine("Reset");
            }
        }
        private void YourCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
           
        }
        void UpdateLists()
        {
            using (var db = new ShoppingContext())
            {

                foreach (var item in db.ProductAction)
                {
                    if(!this.List.Any(x=> x.Name == item.Name))
                        this.List.Add(item);
                        
                }

                this.NumberofItems.Value = this.List.Count();

                foreach (var item in db.BuyersAction)
                {
                    if (!this.UserList.Any(x => x.BuyerId == item.BuyerId))
                        this.UserList.Add(item);
                }

                this.NumberofUsers.Value = this.UserList.Count();

                if(this.NumberofUsers.Value == 0)
                {
                    db.BuyersAction.Add(new Buyers  {FirstName = "Anna", SecondName = "Kozminska" });
                    db.BuyersAction.Add(new Buyers { FirstName = "Michal", SecondName = "Kozminski" });
                    db.SaveChanges();
                    UpdateLists();
                }
                foreach (var item in this.List)
                {
                    var Li = new ObservableCollection<Buyers>();
                    foreach (var us in this.UserList)
                    {
                        Li.Add(us);
                    }
                    if (!this.ListShow.Any(x => x.Name == item.Name))
                        this.ListShow.Add(new ProductsShow {Name =  item.Name,  UserList = Li });
             
                }
            }
        }

       
    }
}