import os
import pandas
import numpy as np
from collections import defaultdict
from scipy.ndimage import gaussian_filter1d


def find_peaks(arr):
    """
    Takes a Gaussian smoothened data 1d Array
    :param arr: list of numbers
    :return: [(index, max_value)], [(index, min_value)]
    """
    max_peaks = []
    min_peaks = []
    prev = arr[0]
    movement_direction = True  # T: Increase, F: Decrease
    for i, next in enumerate(arr[1:]):
        last_movement_direction = movement_direction
        if prev > next:
            movement_direction = False
        if next > prev:
            movement_direction = True
        if movement_direction != last_movement_direction:
            if last_movement_direction == True:
                max_peaks.append(((i - 1), prev))
            else:
                min_peaks.append(((i-1), prev))
        prev = next

    buf = []
    for x in range(len(arr)):
        buf.append(None)
    for data, p in [(max_peaks, 'H'), (min_peaks, 'L')]:
        for (i, v) in data:
            buf[i] = p

    return max_peaks, min_peaks, buf


def read_file(filepath='data/COVID-19/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv'):
    df = pandas.read_csv(filepath)
    df = df.dropna(subset=['Country/Region'])
    return df


def get_daily_data(cum_data):
    r = []
    prev = cum_data[0]
    r.append(prev)
    for next in cum_data[1:]:
        today = next - prev
        r.append(today)
        prev = next
    return r


def create_dir(path):
    exists = os.path.exists(path)
    if not exists:
        os.makedirs(path, exist_ok=True)
        print(f'Create Directory : {path}')


def main():
    df = read_file()
    columns = list(df.columns)
    countries = df['Country/Region'].to_list()
    print(df.shape)

    for country in countries:
        results = []
        country_specific_df = df[df['Country/Region'] == country]
        available_provinces = country_specific_df['Province/State'].to_list()
        available_provinces = [x for x in available_provinces if pandas.isnull(x) == False]
        print("Processing {} having {} Provinces/States".format(country, len(available_provinces)))
        if len(available_provinces) >= 1:
            root_folder = 'region_level'
            p = '{}/{}'.format(root_folder, country)
            create_dir(p)
            for province in available_provinces:
                p = '{}/{}/{}'.format(root_folder, country, province)
                create_dir(p)
        c_results = country_specific_df.values.tolist()
        # Country Level Data Collection and Converted into a DataFrame
        d_data = defaultdict(list)
        for r in c_results:
            x = list(zip(df.columns[4:], r[4:]))
            for dt, c in x:
                d_data[dt].append(c)
        for k, v in d_data.items():
            results.append([country, k, sum(v)])

        r_df = pandas.DataFrame(data=results, columns=['Country','Date','Count'])
        daily_data = get_daily_data(r_df['Count'].to_list())
        r_df['Daily'] = daily_data
        r_df['Smoothed_Daily'] = gaussian_filter1d(daily_data, 4)
        max_peaks, min_peaks, peak_data = find_peaks(r_df['Smoothed_Daily'].to_list())
        r_df['Peak_Information'] = peak_data
        r_df.to_csv('countries/{}.csv'.format(country), header=True, index=False)

        # Province/State level Data Collection and Conversions into DataFrames as Needed
        if len(available_provinces) > 1:
            for province in available_provinces:
                province_filtered_df = country_specific_df[country_specific_df['Province/State'] == province]
                province_results = []
                p_results = province_filtered_df.values.tolist()
                p_data = defaultdict(list)
                for r in p_results:
                    x = list(zip(df.columns[4:], r[4:]))
                    for dt, c in x:
                        p_data[dt].append(c)
                for k, v in p_data.items():
                    province_results.append([country, province, k, sum(v)])

                p_df = pandas.DataFrame(data=province_results, columns=['Country', 'Province', 'Date', 'Count'])
                daily_data = get_daily_data(p_df['Count'].to_list())
                p_df['Daily'] = daily_data
                p_df['Smoothed_Daily'] = gaussian_filter1d(daily_data, 4)
                max_peaks, min_peaks, peak_data = find_peaks(p_df['Smoothed_Daily'].to_list())
                p_df['Peak_Information'] = peak_data
                p_df.to_csv('{}/{}/{}/{}.csv'.format('region_level', country, province, province), header=True, index=False)


main()

