import { Component, Input, Output, EventEmitter } from '@angular/core';
import { WeatherService, ServiceResponse, getBestGuessCityWeatherResult } from '../weather.service'

@Component({
  selector: 'app-form-component',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class FormComponent {
  @Input() cityName: string = "";
  // If TRUE, use test data and rotate the type of result with each call to onSearchButtonClick
  @Input() useForcedResults: boolean = true;
  @Output() newSearchResultAvailableEvent = new EventEmitter<SearchResult>();
  forcedResultType: ForcedResult_Type = ForcedResult_Type.NoResult;
  forcedResultTypeName: string = ForcedResult_Type[ForcedResult_Type.NoResult];
  private callCounter: number = 0;

  constructor(public weatherService: WeatherService) { }

  async onSearchButtonClick() {
    let newForcedResultType: ForcedResult_Type = ForcedResult_Type.NoResult;
    let testSearchResult: SearchResult | null = new SearchResult();
    let actualCityName: string = this.cityName;

    if (this.useForcedResults) {
      this.callCounter += 1;
      if ((this.callCounter % 4) == 0) {
        newForcedResultType = ForcedResult_Type.NoResult;
      }
      else if ((this.callCounter % 4) == 1) {
        newForcedResultType = ForcedResult_Type.ApiSuccessNoData;
      }
      else if ((this.callCounter % 4) == 2) {
        newForcedResultType = ForcedResult_Type.ApiFailed;
      }
      else if ((this.callCounter % 4) == 3) {
        newForcedResultType = ForcedResult_Type.ApiSuccessWithData;
      }

      actualCityName = "Detroit";
      if (newForcedResultType == ForcedResult_Type.NoResult) {
        testSearchResult = null;
      }
      else if (newForcedResultType == ForcedResult_Type.ApiSuccessNoData) {
        testSearchResult.succeeded = true;
        testSearchResult.data = null;
      }
      else if (newForcedResultType == ForcedResult_Type.ApiFailed) {
        testSearchResult.succeeded = false;
        testSearchResult.errorMessage = "A simulated error occured";
      }
      else if (newForcedResultType == ForcedResult_Type.ApiSuccessWithData) {
        testSearchResult.succeeded = true;
        testSearchResult.data = new CityWeather();
        testSearchResult.data.cityName = "Detroit";
        testSearchResult.data.conditionsDescription = "clear sky";
        testSearchResult.data.temperatureActualFarenheit = 20.17;
        testSearchResult.data.temperatureFeelsLikeFarenheit = 6.94;
        testSearchResult.data.currentWindSpeedMph = 12.57;
      }
      this.forcedResultType = newForcedResultType;
      this.forcedResultTypeName = ForcedResult_Type[newForcedResultType];
    }
    else {
      let apiResult: ServiceResponse<getBestGuessCityWeatherResult> | null = null;

      // Was a city specified?
      if ((actualCityName != null) && (actualCityName != "")) {
        // Yes, get the weather for it
        apiResult = await this.weatherService.getBestGuessCityCurrentWeather(actualCityName);
        testSearchResult.succeeded = apiResult.succeeded;
        testSearchResult.errorMessage = apiResult.failureMessage;
        if (apiResult.data != null) {
          testSearchResult.data = new CityWeather();
          testSearchResult.data.cityName = apiResult.data.cityName;
          testSearchResult.data.conditionsDescription = apiResult.data.conditionsDescription;
          testSearchResult.data.currentWindSpeedMph = apiResult.data.currentWindSpeedMph;
          testSearchResult.data.temperatureActualFarenheit = apiResult.data.temperatureActualFarenheit;
          testSearchResult.data.temperatureFeelsLikeFarenheit = apiResult.data.temperatureFeelsLikeFarenheit;
        }
      } else {
        // No, clear the result display
        testSearchResult = null;
      }
    }

    if (testSearchResult != null) {
      if (testSearchResult.succeeded && (testSearchResult.data == null)) {
        // Was a cityName specified?
        if ((actualCityName != null) && (actualCityName != "")) {
          // Yes, translate success with no result to an error indicating city not found
          testSearchResult.succeeded = false;
          testSearchResult.errorMessage = 'A city named ' + this.cityName + ' was not found.';
          testSearchResult.data = null;
        }
        else {
          // No, translate success with no result to no response so result form is not shown
          testSearchResult = null;
        }
      }
    }

    this.newSearchResultAvailableEvent.emit(testSearchResult!);
  }
}

export class SearchResult {
  succeeded: boolean = false;
  errorMessage: string = "";
  data: CityWeather | null = null;
}

export class CityWeather {
  cityName: string = "";
  conditionsDescription: string = "";
  temperatureActualFarenheit: number = 0;
  temperatureFeelsLikeFarenheit: number = 0;
  currentWindSpeedMph: number = 0;
}

export enum ForcedResult_Type {
  NoResult,
  ApiSuccessNoData,
  ApiFailed,
  ApiSuccessWithData
}
