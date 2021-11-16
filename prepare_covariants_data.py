import os
import json

import pandas
import matplotlib.pyplot as plt
import matplotlib.dates as mdates


def read_covariant_source_data(filetype='json', data_dir='data/covariants/cluster_tables/'):
    files = [f for f in os.listdir(data_dir) if f.endswith(filetype)]
    return files


def read_file(parent_dir, filename):
    full_file_dir = f'{parent_dir}{filename}'
    f = open(full_file_dir)
    data = json.load(f)
    return data


def read_raw_data_and_generate_intermediate_combined_file():
    src_dir = 'data/covariants/cluster_tables/'
    files = read_covariant_source_data(data_dir=src_dir)
    print(files)
    file_result = {}
    for file in files:
        s = file.split('_')[0]
        file_result[s] = read_file(src_dir, file)

    out_dir = 'data/'
    out_file = 'covariants.json'
    print("Writing to File ...")
    with open(f'{out_dir}{out_file}', 'w', encoding='utf-8') as f:
        json.dump(file_result, f, indent=4)
    print(f"Data written to file {out_dir}{out_file}.")


def identify_total_columns_in_output_file(data):
    available_variants = list(data.keys())
    unique_columns = set()
    ignore_covid_variants = ['EUClusters', 'perVariant', 'SwissClusters', 'USAClusters']
    print("Identifying the week bounds for the data ...")
    for covid_variant in available_variants:
        if covid_variant in ignore_covid_variants:
            continue
        countries = list(data[covid_variant].keys())
        for country in countries:
            additional_columns = data[covid_variant][country]['week']
            unique_columns = unique_columns.union(additional_columns)
    # Sort the list
    possible_columns = list(unique_columns)
    possible_columns.sort()
    print("Identified week bounds, these become columns of the time series data.")
    return possible_columns


def process_intermediate_file_to_usable_data_file():
    intermediate_dir = 'data/'
    intermediate_file = 'covariants.json'
    f = open(f'{intermediate_dir}{intermediate_file}')
    data = json.load(f)
    available_variants = list(data.keys())
    column_headers = ['Variant', 'Country']
    new_columns = identify_total_columns_in_output_file(data)
    all_columns = column_headers + new_columns
    # There are 4 types of files
    possible_outputs = ['total_sequences', 'cluster_sequences', 'unsmoothed_cluster_sequences', 'unsmoothed_total_sequences']
    ignore_covid_variants = ['EUClusters', 'perVariant', 'SwissClusters', 'USAClusters']
    for output_type in possible_outputs:
        print('Identifying {} results for all COVID variants. {}/{}'.format(output_type, possible_outputs.index(output_type) + 1, len(possible_outputs)))
        results = []
        for covid_variant in available_variants:
            if covid_variant in ignore_covid_variants:
                continue
            countries = list(data[covid_variant].keys())
            for country in countries:
                weeks_reported = data[covid_variant][country]['week']
                data_output_type = data[covid_variant][country][output_type]
                processable_data = list(zip(weeks_reported, data_output_type))
                row = [0] * len(all_columns)
                if 'total' in output_type:
                    row[0] = 'All'
                else:
                    row[0] = covid_variant
                row[1] = country
                for (week_date, count) in processable_data:
                    index = all_columns.index(week_date)
                    row[index] = count
                results.append(row)
        df = pandas.DataFrame(data=results, columns=all_columns)
        df = df.drop_duplicates(subset=column_headers)
        df.to_csv(f'{intermediate_dir}covariants_{output_type}.csv', header=True, index=False)


def plot_covariant_heatmaps_and_area_plots(output_dir='data/covariants_images/'):
    variant_data = pandas.read_csv('data/covariants_cluster_sequences.csv')
    totals = pandas.read_csv('data/covariants_total_sequences.csv')

    column_headers = ['Variant', 'Country']
    weeks_data = list(variant_data.columns)[len(column_headers):]

    chosen_countries = list(totals['Country'].unique())
    ignored_countries = ['Chile', 'South Africa', 'Bangladesh']

    for country in chosen_countries:
        if country in ignored_countries:
            continue
        print(f"Generating the Data outputs (file, figures) for {country}")
        total_df = totals[totals['Country'] == country]
        total_row = []
        for i, r in total_df.iterrows():
            total_row = list(r[len(column_headers):])

        # Figure 1: Absolute Line Plot Figures
        fig, ax = plt.subplots(nrows=1, ncols=1)
        # Major ticks every 1 month
        fmt_one_month = mdates.MonthLocator(interval=1)
        ax.xaxis.set_major_locator(fmt_one_month)
        ax.plot(weeks_data, total_row, label='Total (Absolute)')
        variant_df = variant_data[variant_data['Country'] == country]
        for index, variant_row in variant_df.iterrows():
            variant_row = list(variant_row)
            variant_name = variant_row[0]
            # Since these are mutations!
            if variant_name.startswith('S') or variant_name.startswith('21A.21B') or 'ORF1a' in variant_name:
                continue
            variant_row_data = variant_row[len(column_headers):]
            ax.plot(weeks_data, variant_row_data, label=variant_name)
        ax.legend(fancybox=True, ncol=5, bbox_to_anchor=(0, -0.5), loc='center left')
        plt.savefig(f'{output_dir}{country}.png', bbox_inches='tight')

        # Figure 2: Stacked Area Plot Figures
        fig, ax = plt.subplots(nrows=1, ncols=1)
        # Major ticks every 1 month
        fmt_one_month = mdates.MonthLocator(interval=1)
        ax.xaxis.set_major_locator(fmt_one_month)
        variant_df = variant_data[variant_data['Country'] == country]
        stacked_data = {}
        for index, variant_row in variant_df.iterrows():
            variant_row = list(variant_row)
            variant_name = variant_row[0]
            if variant_name.startswith('S') or variant_name.startswith('21A.21B') or 'ORF1a' in variant_name:
                continue
            variant_row_data = variant_row[len(column_headers):]
            tmp = list(zip(variant_row_data, total_row))
            r = []
            for (variant_count, total) in tmp:
                if total > 0:
                    t = float(variant_count)/float(total)
                    if t <= 1.0:
                        r.append(t)
                    else:
                        print(f'{country} : {variant_row} has data {variant_count} / {total}')
                else:
                    r.append(0.0)
            stacked_data[variant_name] = r
        s_df = pandas.DataFrame(data=stacked_data)
        s_df.to_csv(f'data/covariants_country_clusters/{country}.csv', header=True, index=False)
        s_df.index = weeks_data
        ax = s_df.plot.area(stacked=True)
        ax.legend(fancybox=True, ncol=5, bbox_to_anchor=(0, -0.5), loc='center left')
        plt.xticks(rotation=90)
        plt.savefig(f'{output_dir}{country}_area.png', bbox_inches='tight')
        plt.close('all')


def pipeline():
    # Step 1: Use the raw data from Covariants data and combine them into a different format
    read_raw_data_and_generate_intermediate_combined_file()
    # Step 2: Use the intermediate raw data generated to convert it into a pivot table.
    process_intermediate_file_to_usable_data_file()
    # Step 3: Generate the output figures for select countries.
    output_dirs = ['data/covariants_images/', 'data/covariants_country_clusters']
    for output_dir in output_dirs:
        if not os.path.exists(output_dir):
            print(f"Making the output directory for images {output_dir}")
            os.makedirs(output_dir)
    plot_covariant_heatmaps_and_area_plots()


if __name__ == '__main__':
    pipeline()


