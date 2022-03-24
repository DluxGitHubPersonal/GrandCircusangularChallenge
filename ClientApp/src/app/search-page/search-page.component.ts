import { Component } from '@angular/core';
import { FormComponent, SearchResult, CityWeather } from '../form/form.component';
import { HeaderComponent } from '../header/header.component';
import { ResultComponent } from '../result/result.component';

@Component({
  selector: 'app-search-page-component',
  templateUrl: './search-page.component.html',
  styleUrls: ['./search-page.component.css']
})
export class SearchPageComponent {
  public receivedSearchResult: SearchResult | null = null;
  public onReceivedSearchResult(newSearchResult: SearchResult) {
    this.receivedSearchResult = newSearchResult;
  }

  constructor() { }

}
