import { Inject, Injectable } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpErrorResponse,
} from '@angular/common/http';


// Angular client for the ASP.NET Core WeatherForecastController web service
@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private aspBaseUrl: string = "";

  constructor(private http: HttpClient, @Inject('BASE_URL') newBaseUrl: string) {
    this.aspBaseUrl = newBaseUrl;
  }

  async getBestGuessCityCurrentWeather(cityName: string): Promise<ServiceResponse<getBestGuessCityWeatherResult>> {
    let callResult: ServiceResponse<getBestGuessCityWeatherResult> | null = null;
    let targetRequestUri: string = this.aspBaseUrl + 'weatherforecast/GetBestGuessCityCurrentWeather?cityName=' + cityName;
    // SPA proxy is case sensitive!
    try {
      callResult = await this.http.get<ServiceResponse<getBestGuessCityWeatherResult>>(targetRequestUri).toPromise()
    }
    catch (unexpectedException) {
      callResult = new ServiceResponse<getBestGuessCityWeatherResult>();
      callResult.succeeded = false;
      if (unexpectedException instanceof HttpErrorResponse) {
        let unexpectedExceptionHttp: HttpErrorResponse = unexpectedException;
        callResult.failureMessage = 'Error sending http request ' + targetRequestUri+': '+unexpectedExceptionHttp.message;
      }
      // callResult.failureMessage = unexpectedException.Error;
    }
    if (callResult != null) {
      if (callResult.succeeded) {
        if (callResult.data != null)
          console.log("Call succeeded with result: " + callResult.data.cityName);
        else
          console.log("Call succeeded with NO data.  Failure message:  " + callResult.failureMessage);
      }
      else {
        console.log("Call failed:  " + callResult.failureMessage);
      }
    }
    else {
      console.log("Call complete, result NULL");
    }

    return callResult;
  }
}

// SYNCHRONIZE with WeatherForecastController.CityWeatherInfo
export class getBestGuessCityWeatherResult {
  public cityName: string = "";
  public stateName: string = "";
  public countryId: string = "";
  public cityCode: number = 0;
  public conditionsDescription: string = "";
  public conditionsStandardId: number = 0;
  public temperatureActualFarenheit: number = 0;
  public temperatureFeelsLikeFarenheit: number = 0;
  public currentWindSpeedMph: number = 0;
}

// SYNCHRONIZE with WeatherForecastController.ServiceResponse<T>
export class ServiceResponse<T extends object> {
  public succeeded: boolean = false;
  public failureMessage: string = "";
  public data: T | null = null;
}
