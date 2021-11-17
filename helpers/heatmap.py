import pandas


__NON_TIME_SERIES_COLUMNS = ['DataType', 'Admin2', 'Admin1', 'Admin0', 'Population', 'CaseCount']
__BAD_DATA_COUNTRIES = ['Guernsey']
__NORMALIZED_PER_VALUE = 100000.0


def process_heatmap_data(df):
    country_wise_result = {}
    for index, row in df.iterrows():
        country_name = row['Admin0']
        if country_name in __BAD_DATA_COUNTRIES:
            continue
        population = row['Population']
        total_case_count = row['CaseCount']
        day_wise_data = row[len(__NON_TIME_SERIES_COLUMNS):]
        day_wise_data_proportion = [float(d)/float(population) * __NORMALIZED_PER_VALUE for d in day_wise_data]
        country_wise_result[country_name] = day_wise_data_proportion
    return country_wise_result


def get_normalized_per_population_value():
    return __NORMALIZED_PER_VALUE


def get_non_time_series_columns():
    return __NON_TIME_SERIES_COLUMNS