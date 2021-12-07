from collections import defaultdict

import pandas

from data.vaccines import get_cdc_data, generate_cdc_data_timeseries_daily, generate_fips_mappings, merge_timeseries_mappings


_COLUMN_BASE = ['FIPS', 'Recip_County', 'Recip_State']


def prepare_timeline_data(df):
    dates = list(set(df['Date'].to_list()))
    dates.sort()
    # Structure: 'FIPS:Recip_County:Recip_State' : ['Series Complete Yes'...]
    unique_fips_codes = list(set(df['FIPS'].to_list()))
    print(len(unique_fips_codes), ' number of FIPS codes')

    columns = ['FIPS'] + dates

    rows = []
    for i, fips_code in enumerate(unique_fips_codes):
        if i % 100 == 0:
            print(f"Processed {i} FIPS entries")
        f_df = df[df['FIPS'] == fips_code]
        zero_count = [0] * len(dates)
        for index, row in f_df.iterrows():
            dt = row['Date']
            index = dates.index(dt)
            zero_count[index] = row['Series_Complete_Yes']
        r = [fips_code] + zero_count
        rows.append(r)

    t_df = pandas.DataFrame(data=rows, columns=columns)
    t_df.to_csv('vaccine_timeline_fully_vaccinated.csv', header=True, index=False)
    return t_df


if __name__ == '__main__':
    # df = generate_fips_mappings()
    df = merge_timeseries_mappings()
    print(df)
    # print(df.columns)
    # r = prepare_timeline_data(df)
    # print(r)
